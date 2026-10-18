// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Globalization;
using System.Security.Claims;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OroIdentityServer.Application.Modules.Mfa.Commands;
using OroIdentityServer.Application.Modules.Mfa.Queries;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Application.Modules.UserSessions.Queries;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;
using IdentityServer.Services;
using OroIdentityServer.Server.Authentication;

namespace OroIdentityServer.Server.Endpoints;

public static class AuthEndpoints
{
    public const string SessionTokenClaimType = "session_token";

    public sealed record LoginRequest(string LoginIdentifier, string Password, string? ReturnUrl);
    public sealed record ChangePasswordInputModel(string NewPassword, string ConfirmPassword, string? ReturnUrl);
    public sealed record MfaCodeInputModel(string Code);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            HttpContext http,
            [FromForm] LoginInputModel loginInput,
            [FromServices] IAdminPasswordSignInService signInService,
            [FromServices] IQueryDispatcher queryDispatcher,
            [FromServices] ICommandDispatcher commandDispatcher,
            CancellationToken ct) =>
        {
            var principal = await signInService.SignInAsync(loginInput.LoginIdentifier, loginInput.Password, ct);
            if (principal is null)
            {
                var errorReturnUrl = Uri.EscapeDataString(loginInput.ReturnUrl ?? string.Empty);
                return Results.Redirect($"/Account/Login?error=invalid&ReturnUrl={errorReturnUrl}");
            }

            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Redirect("/Account/Login?error=invalid");

            // The password is only half of the story: once MFA is due, no session is created and
            // no admin cookie is written. The pending challenge cookie is the only thing that
            // survives, and it cannot sign anyone in.
            var requirement = await queryDispatcher.SendAsync(new IsMfaRequiredQuery(userId), ct);
            if (requirement.IsRequired)
            {
                if (requirement.RequiresEnrollment)
                    await commandDispatcher.SendAsync(new StartMfaEnrollmentCommand(userId), ct);

                var issued = await commandDispatcher.SendAsync(new CreateMfaLoginChallengeCommand(
                    userId,
                    requirement.RequiresEnrollment,
                    loginInput.ReturnUrl,
                    http.Connection.RemoteIpAddress?.ToString(),
                    http.Request.Headers.UserAgent.ToString()), ct);

                await CookieAuthHandlerSetup.StartMfaChallengeAsync(http, issued.Token);

                return Results.Redirect(requirement.RequiresEnrollment ? "/Account/MfaSetup" : "/Account/Mfa");
            }

            return await CompleteSignInAsync(http, principal, userId, loginInput.ReturnUrl, commandDispatcher, ct);
        }).DisableAntiforgery();

        // Second step of the login: the password already checked out, the authenticator must now
        // confirm. Only on success is the admin cookie issued and the UserSession created.
        group.MapPost("/mfa", async (
            HttpContext http,
            [FromForm] MfaCodeInputModel input,
            [FromServices] IAdminPasswordSignInService signInService,
            [FromServices] ICommandDispatcher commandDispatcher,
            CancellationToken ct) =>
        {
            var challengeToken = await CookieAuthHandlerSetup.GetPendingMfaChallengeTokenAsync(http);
            if (string.IsNullOrEmpty(challengeToken) || string.IsNullOrWhiteSpace(input.Code))
                return Results.Redirect("/Account/Login?error=mfa");

            var verification = await commandDispatcher.SendAsync(
                new VerifyMfaChallengeCommand(challengeToken, input.Code), ct);

            if (!verification.Succeeded)
            {
                return Results.Redirect(BuildMfaFailureRedirect(verification));
            }

            await CookieAuthHandlerSetup.ClearMfaChallengeAsync(http);

            var principal = await signInService.BuildPrincipalForSignedInUserAsync(verification.UserId, ct);
            if (principal is null)
                return Results.Redirect("/Account/Login?error=invalid");

            return await CompleteSignInAsync(http, principal, verification.UserId, verification.ReturnUrl, commandDispatcher, ct);
        }).DisableAntiforgery();

        // Enrolment path (tenant policy): confirm the first code against the freshly issued
        // secret, which is also what consumes the time step so the enrolment code cannot be
        // replayed as a login code.
        group.MapPost("/mfa/enroll", async (
            HttpContext http,
            [FromForm] MfaCodeInputModel input,
            [FromServices] IAdminPasswordSignInService signInService,
            [FromServices] IQueryDispatcher queryDispatcher,
            [FromServices] ICommandDispatcher commandDispatcher,
            CancellationToken ct) =>
        {
            var challengeToken = await CookieAuthHandlerSetup.GetPendingMfaChallengeTokenAsync(http);
            if (string.IsNullOrEmpty(challengeToken) || string.IsNullOrWhiteSpace(input.Code))
                return Results.Redirect("/Account/Login?error=mfa");

            // The user id comes from the challenge, never from the form, so a submission cannot
            // confirm a credential belonging to somebody else.
            var challenge = await queryDispatcher.SendAsync(new GetMfaChallengeQuery(challengeToken), ct);
            if (!challenge.Found || !challenge.IsUsable)
                return Results.Redirect("/Account/MfaSetup?error=expired");

            try
            {
                await commandDispatcher.SendAsync(new ConfirmMfaEnrollmentCommand(challenge.UserId, input.Code), ct);
            }
            catch (InvalidOperationException)
            {
                return Results.Redirect("/Account/MfaSetup?error=invalid");
            }

            var completion = await commandDispatcher.SendAsync(
                new CompleteMfaEnrollmentChallengeCommand(challengeToken), ct);
            if (!completion.Succeeded)
                return Results.Redirect("/Account/MfaSetup?error=expired");

            await CookieAuthHandlerSetup.ClearMfaChallengeAsync(http);

            var principal = await signInService.BuildPrincipalForSignedInUserAsync(challenge.UserId, ct);
            if (principal is null)
                return Results.Redirect("/Account/Login?error=invalid");

            return await CompleteSignInAsync(http, principal, challenge.UserId, challenge.ReturnUrl, commandDispatcher, ct);
        }).DisableAntiforgery();

        // Second factor still pending: the QR, served as PNG so the statically rendered page does
        // not need JavaScript. Only reachable while holding the pending challenge cookie.
        group.MapGet("/mfa/qr", async (
            HttpContext http,
            [FromServices] IQueryDispatcher queryDispatcher,
            [FromServices] IQrCodeGenerator qrCodeGenerator,
            CancellationToken ct) =>
        {
            var challengeToken = await CookieAuthHandlerSetup.GetPendingMfaChallengeTokenAsync(http);
            if (string.IsNullOrEmpty(challengeToken))
                return Results.NotFound();

            var challenge = await queryDispatcher.SendAsync(new GetMfaChallengeQuery(challengeToken), ct);
            if (!challenge.Found)
                return Results.NotFound();

            try
            {
                var payload = await queryDispatcher.SendAsync(
                    new GetMfaEnrollmentSecretQuery(challenge.UserId), ct);

                return Results.File(qrCodeGenerator.GeneratePng(payload.ProvisioningUri), "image/png");
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        }).DisableAntiforgery();

        group.MapMethods("/logout", [HttpMethod.Get.Method, HttpMethod.Post.Method], async (
            HttpContext http,
            [FromServices] ICommandDispatcher commandDispatcher,
            [FromServices] IQueryDispatcher queryDispatcher,
            [FromServices] IOpenIddictAuthorizationManager authorizationManager,
            [FromServices] IOpenIddictTokenManager tokenManager,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("AuthEndpoints");

            if (http.User.Identity?.IsAuthenticated == true)
            {
                var sessionToken = http.User.FindFirstValue(SessionTokenClaimType);
                var subject = http.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(sessionToken))
                {
                    try
                    {
                        var sessions = await queryDispatcher.SendAsync(
                            new GetUserSessionsByUserQuery(Guid.Parse(subject!)), CancellationToken.None);
                        var session = sessions?.FirstOrDefault(s => s.SessionToken == sessionToken);
                        if (session is not null)
                        {
                            await commandDispatcher.SendAsync(
                                new DeactivateUserSessionCommand(session.Id.Value), CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to deactivate user session on logout");
                    }
                }

                if (!string.IsNullOrEmpty(subject))
                {
                    try
                    {
                        await RevokeAllTokensForSubjectAsync(authorizationManager, tokenManager, subject, logger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to revoke OpenIddict tokens on logout for subject {Subject}", subject);
                    }
                }
            }

            await http.SignOutAsync(CookieAuthHandlerSetup.AdminScheme);
            await http.SignOutAsync(CookieAuthHandlerSetup.MfaPendingScheme);
            return Results.Redirect("/Account/Login");
        }).DisableAntiforgery();

        group.MapPost("/change-password", async (
            HttpContext http,
            [FromForm] ChangePasswordInputModel input,
            [FromServices] IUserRepository userRepository,
            [FromServices] ISecurityUserRepository securityUserRepository,
            [FromServices] IPasswordHasher passwordHasher,
            [FromServices] IAdminPasswordSignInService signInService,
            CancellationToken ct) =>
        {
            if (http.User.Identity?.IsAuthenticated != true)
                return Results.Redirect("/Account/Login");

            if (string.IsNullOrWhiteSpace(input.NewPassword) || input.NewPassword != input.ConfirmPassword)
                return Results.Redirect("/Account/ChangePassword?error=mismatch");

            var userId = Guid.Parse(http.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await userRepository.GetUserByIdAsync(new(userId), ct);
            if (user?.SecurityUserId is null)
                return Results.Redirect("/Account/Login");

            var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId.Value, ct);
            if (securityUser is null)
                return Results.Redirect("/Account/Login");

            securityUser.ChangePassword(await passwordHasher.HashPassword(input.NewPassword));
            await securityUserRepository.UpdateSecurityUserAsync(securityUser, ct);

            var principal = await signInService.RefreshPrincipalAsync(userId, ct);
            if (principal is not null)
            {
                var existingSessionToken = http.User.FindFirstValue(SessionTokenClaimType);
                if (!string.IsNullOrEmpty(existingSessionToken))
                    principal.Identities.First().AddClaim(new Claim(SessionTokenClaimType, existingSessionToken));

                await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);
            }

            var target = string.IsNullOrWhiteSpace(input.ReturnUrl) ? "/" : input.ReturnUrl;
            return Results.Redirect(target);
        }).DisableAntiforgery();

        return app;
    }

    /// <summary>
    /// Turns a failed code submission into the query string the <c>/Account/Mfa</c> page knows how
    /// to render. Keeping the mapping here means the page never has to understand
    /// <see cref="MfaFailureReason"/>.
    /// </summary>
    internal static string BuildMfaFailureRedirect(MfaChallengeVerification verification)
    {
        if (verification.RequiresEnrollment
            || verification.FailureReason == MfaFailureReason.SecretUnavailable)
        {
            return "/Account/MfaSetup?error=secret";
        }

        if (verification.FailureReason == MfaFailureReason.ChallengeExpired)
        {
            return "/Account/Mfa?error=expired";
        }

        // Two different locks, two different pages: "lockedout" carries a countdown the page
        // renders, "locked" tells the user to start over with the password step.
        var reason = verification.FailureReason switch
        {
            MfaFailureReason.LockedOut => "lockedout",
            MfaFailureReason.ChallengeLocked => "locked",
            _ => "invalid"
        };

        return $"/Account/Mfa?error={reason}&attempts={verification.AttemptsRemaining}"
             + $"&retryAfter={Math.Max(0, verification.RetryAfterSeconds)}";
    }

    /// <summary>
    /// The single place that turns a verified identity into a signed-in session: writes the admin
    /// cookie, opens the <c>UserSession</c> row and decides where to send the user next. Shared by
    /// the password step and by both second-factor steps so they cannot drift apart.
    /// </summary>
    internal static async Task<IResult> CompleteSignInAsync(
        HttpContext http,
        ClaimsPrincipal principal,
        Guid userId,
        string? returnUrl,
        ICommandDispatcher commandDispatcher,
        CancellationToken ct)
    {
        var sessionToken = Guid.NewGuid().ToString("N");
        principal.Identities.First().AddClaim(new Claim(SessionTokenClaimType, sessionToken));

        await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);

        var userAgent = http.Request.Headers.UserAgent.ToString();
        var device = ParseDeviceFromUserAgent(userAgent);
        var ipAddress = http.Connection.RemoteIpAddress?.ToString();

        try
        {
            await commandDispatcher.SendAsync(new CreateUserSessionCommand(
                userId, device, sessionToken,
                DateTime.UtcNow.Add(CookieAuthHandlerSetup.SessionLifetime),
                ipAddress, userAgent, null,
                CurrentCulture()), ct);
        }
        catch (Exception ex)
        {
            var logger = http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AuthEndpoints");
            logger.LogError(ex, "Failed to create user session for user {UserId}", userId);
        }

        var safeReturnUrl = SanitizeReturnUrl(returnUrl);

        if (principal.HasClaim(c => c.Type == AdminPasswordSignInService.MustChangePasswordClaimType))
        {
            if (string.IsNullOrEmpty(safeReturnUrl))
                return Results.Redirect("/Account/ChangePassword");

            return Results.Redirect($"/Account/ChangePassword?ReturnUrl={Uri.EscapeDataString(safeReturnUrl)}");
        }

        return Results.Redirect(string.IsNullOrEmpty(safeReturnUrl) ? "/" : safeReturnUrl);
    }

    /// <summary>
    /// Only same-origin relative paths are accepted as a return target; anything else (an
    /// absolute URL to another host) would turn the login form into an open redirect.
    /// </summary>
    internal static string? SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;

        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal) || returnUrl.StartsWith("/\\", StringComparison.Ordinal))
            return null;

        return returnUrl;
    }

    private static string? CurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        return string.IsNullOrWhiteSpace(culture.Name) ? null : culture.Name;
    }

    internal static string ParseDeviceFromUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown Device";

        var ua = userAgent.AsSpan();

        string os = "Unknown OS";
        if (ua.Contains("Windows".AsSpan(), StringComparison.OrdinalIgnoreCase)) os = "Windows";
        else if (ua.Contains("Mac OS X".AsSpan(), StringComparison.OrdinalIgnoreCase)) os = "macOS";
        else if (ua.Contains("Linux".AsSpan(), StringComparison.OrdinalIgnoreCase)) os = "Linux";
        else if (ua.Contains("Android".AsSpan(), StringComparison.OrdinalIgnoreCase)) os = "Android";
        else if (ua.Contains("iPhone".AsSpan(), StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad".AsSpan(), StringComparison.OrdinalIgnoreCase)) os = "iOS";

        string browser = "Unknown Browser";
        if (ua.Contains("Edg/".AsSpan(), StringComparison.OrdinalIgnoreCase)) browser = "Edge";
        else if (ua.Contains("Chrome".AsSpan(), StringComparison.OrdinalIgnoreCase) && !ua.Contains("Edg".AsSpan(), StringComparison.OrdinalIgnoreCase)) browser = "Chrome";
        else if (ua.Contains("Firefox".AsSpan(), StringComparison.OrdinalIgnoreCase)) browser = "Firefox";
        else if (ua.Contains("Safari".AsSpan(), StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome".AsSpan(), StringComparison.OrdinalIgnoreCase)) browser = "Safari";

        return $"{browser} on {os}";
    }

    internal static async Task RevokeAllTokensForSubjectAsync(
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictTokenManager tokenManager,
        string subject,
        ILogger logger)
    {
        await foreach (var authorization in authorizationManager.FindBySubjectAsync(subject))
        {
            try { await authorizationManager.TryRevokeAsync(authorization); }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke authorization for subject {Subject}", subject); }
        }

        await foreach (var token in tokenManager.FindBySubjectAsync(subject))
        {
            try { await tokenManager.TryRevokeAsync(token); }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke token for subject {Subject}", subject); }
        }
    }
}