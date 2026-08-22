// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Application.Modules.UserSessions.Queries;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Server.Authentication;

namespace OroIdentityServer.Server.Endpoints;

public static class AuthEndpoints
{
    public const string SessionTokenClaimType = "session_token";

    public sealed record LoginRequest(string LoginIdentifier, string Password, string? ReturnUrl);
    public sealed record ChangePasswordInputModel(string NewPassword, string ConfirmPassword, string? ReturnUrl);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            HttpContext http,
            [FromForm] LoginInputModel loginInput,
            [FromServices] AdminPasswordSignInService signInService,
            [FromServices] ICommandDispatcher commandDispatcher,
            CancellationToken ct) =>
        {
            var principal = await signInService.SignInAsync(loginInput.LoginIdentifier, loginInput.Password, ct);
            if (principal is null)
            {
                var errorReturnUrl = Uri.EscapeDataString(loginInput.ReturnUrl ?? string.Empty);
                return Results.Redirect($"/Account/Login?error=invalid&ReturnUrl={errorReturnUrl}");
            }

            var sessionToken = Guid.NewGuid().ToString("N");
            principal.Identities.First().AddClaim(new Claim(SessionTokenClaimType, sessionToken));

            await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);

            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is not null && Guid.TryParse(userIdClaim, out var userId))
            {
                var userAgent = http.Request.Headers.UserAgent.ToString();
                var device = ParseDeviceFromUserAgent(userAgent);
                var ipAddress = http.Connection.RemoteIpAddress?.ToString();

                try
                {
                    await commandDispatcher.SendAsync(new CreateUserSessionCommand(
                        userId, device, sessionToken,
                        DateTime.UtcNow.AddHours(8),
                        ipAddress, userAgent, null), ct);
                }
                catch (Exception ex)
                {
                    var logger = http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AuthEndpoints");
                    logger.LogError(ex, "Failed to create user session for user {UserId}", userId);
                }
            }

            if (principal.HasClaim(c => c.Type == AdminPasswordSignInService.MustChangePasswordClaimType))
            {
                if (string.IsNullOrWhiteSpace(loginInput.ReturnUrl))
                    return Results.Redirect("/Account/ChangePassword");

                var changePasswordReturnUrl = Uri.EscapeDataString(loginInput.ReturnUrl);
                return Results.Redirect($"/Account/ChangePassword?ReturnUrl={changePasswordReturnUrl}");
            }

            var target = string.IsNullOrWhiteSpace(loginInput.ReturnUrl) ? "/" : loginInput.ReturnUrl;
            return Results.Redirect(target);
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
            return Results.Redirect("/Account/Login");
        }).DisableAntiforgery();

        group.MapPost("/change-password", async (
            HttpContext http,
            [FromForm] ChangePasswordInputModel input,
            [FromServices] IUserRepository userRepository,
            [FromServices] ISecurityUserRepository securityUserRepository,
            [FromServices] IPasswordHasher passwordHasher,
            [FromServices] AdminPasswordSignInService signInService,
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
