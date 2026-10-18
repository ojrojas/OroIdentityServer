using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using IdentityServer.Services;
using OroIdentityServer.Server.Authentication;
using OroIdentityServer.Server.Helpers;
using OroIdentityServer.Shared.Authorization;
using static OpenIddict.Abstractions.OpenIddictConstants;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.Users.Queries;
using OroIdentityServer.Application.Modules.Tenants.Queries;
using OroIdentityServer.Application.Modules.Roles.Queries;
using OroIdentityServer.Application.Modules.Permissions.Queries;
using OroIdentityServer.Application.Modules.Diagnostics.Commands;
using OroIdentityServer.Core.Modules.Diagnostics.Enums;
using System.Collections.Immutable;

namespace OroIdentityServer.Server.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IAdminPasswordSignInService _signInService;
    private readonly ITokenPrincipalFactory _tokenPrincipalFactory;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictTokenManager tokenManager,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        IAdminPasswordSignInService signInService,
        ITokenPrincipalFactory tokenPrincipalFactory)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _tokenManager = tokenManager;
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _signInService = signInService;
        _tokenPrincipalFactory = tokenPrincipalFactory;
    }

    private async Task LogValidationAsync(AuthValidationEventType eventType, bool succeeded, Guid? userId, string? clientId, string? scopes, string? failureReason, CancellationToken ct)
    {
        try
        {
            await _commandDispatcher.SendAsync(new LogAuthValidationCommand(eventType, succeeded, userId, clientId, scopes, HttpContext.Connection.RemoteIpAddress?.ToString(), failureReason), ct);
        }
        catch { /* auditing must never break the auth flow */ }
    }

    /// <summary>
    /// Builds the claims identity that OpenIddict turns into tokens.
    /// </summary>
    /// <remarks>
    /// The claim set comes from <see cref="AdminPasswordSignInService"/> - the same builder that
    /// produces the admin cookie - rather than being assembled again here. That is what makes the
    /// two entry points equivalent: a relying party that presents the access token to /api gets
    /// exactly the entitlements the browser console would have, including
    /// <see cref="AuthorizationClaimTypes.IsMasterAdmin"/> and the hierarchy claims that
    /// <c>MasterAdminOnly</c> / <c>CanManageHierarchy</c> evaluate. Assembling the identity here
    /// from scratch - as this controller used to - silently dropped them, which is why every
    /// token-authenticated request to those endpoints came back 403.
    /// Claim types are remapped to their OpenIddict short names (<c>sub</c>, <c>name</c>,
    /// <c>email</c>, <c>role</c>) because that is what the OIDC clients expect to read.
    /// </remarks>
    private async Task<ClaimsIdentity> BuildTokenIdentityAsync(Guid userId, CancellationToken cancellationToken)
    {
        var principal = await _signInService.BuildPrincipalForSignedInUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        foreach (var claim in principal.Claims)
        {
            identity.AddClaim(MapClaim(claim));
        }

        // sub is mandatory in an OIDC response and the cookie principal's NameIdentifier is the
        // one place the subject is guaranteed to be.
        identity.SetClaim(Claims.Subject, userId.ToString());

        return identity;
    }

    private static Claim MapClaim(Claim claim) => claim.Type switch
    {
        // The cookie uses the long ClaimTypes names; tokens must use the short OIDC ones.
        System.Security.Claims.ClaimTypes.NameIdentifier => new Claim(Claims.Subject, claim.Value),
        System.Security.Claims.ClaimTypes.Name => new Claim(Claims.Name, claim.Value),
        System.Security.Claims.ClaimTypes.Email => new Claim(Claims.Email, claim.Value),
        System.Security.Claims.ClaimTypes.Role => new Claim(Claims.Role, claim.Value),
        _ => claim
    };

    /// <summary>
    /// Applies the scopes, the resource list and the destination rules that decide which claim
    /// reaches the access token and which reaches the id token.
    /// </summary>
    private Task CompleteTokenIdentityAsync(ClaimsIdentity identity, IEnumerable<string> requestedScopes, CancellationToken cancellationToken)
        => _tokenPrincipalFactory.ApplyScopesAndDestinationsAsync(identity, requestedScopes, cancellationToken);

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user principal stored in the authentication cookie and redirect
        // the user agent to the login page (or to an external provider) in the following cases:
        //
        //  - If the user principal can't be extracted or the cookie is too old.
        //  - If prompt=login was specified by the client application.
        //  - If max_age=0 was specified by the client application (max_age=0 is equivalent to prompt=login).
        //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
        //
        // For scenarios where the default authentication handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        var result = await HttpContext.AuthenticateAsync();
        if (result is not { Succeeded: true } ||
            ((request.HasPromptValue(PromptValues.Login) || request.MaxAge is 0 ||
             (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
              TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))) &&
            TempData["IgnoreAuthenticationChallenge"] is null or false))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPromptValue(PromptValues.None))
            {
                await LogValidationAsync(AuthValidationEventType.AuthorizeDenied, false, null, request.ClientId, null, "login_required", cancellationToken);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            // To avoid endless login endpoint -> authorization endpoint redirects, a special temp data entry is
            // used to skip the challenge if the user agent has already been redirected to the login endpoint.
            //
            // Note: this flag doesn't guarantee that the user has accepted to re-authenticate. If such a guarantee
            // is needed, the existing authentication cookie MUST be deleted AND revoked (e.g using ASP.NET Core
            // Identity's security stamp feature with an extremely short revalidation time span) before triggering
            // a challenge to redirect the user agent to the login endpoint.
            TempData["IgnoreAuthenticationChallenge"] = true;

            // For scenarios where the default challenge handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form : Request.Query)
            });
        }

        var subject = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new InvalidOperationException("The subject claim cannot be retrieved.");

        // Retrieve the profile of the logged in user.
        var user = await _queryDispatcher.SendAsync(new GetUserByIdQuery(Guid.Parse(subject)), cancellationToken) ??
           throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken: cancellationToken) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
              query: (Subject: user.Data.Id.Value.ToString(),
             ApplicationId: await _applicationManager.GetIdAsync(application, cancellationToken),
             Status: Statuses.Valid,
             Type: AuthorizationTypes.Permanent, RequiredScopes: request.GetScopes()),
             cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        switch (await _applicationManager.GetConsentTypeAsync(application, cancellationToken))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case ConsentTypes.External when authorizations.Count is 0:
                await LogValidationAsync(AuthValidationEventType.AuthorizeDenied, false, user.Data.Id.Value, request.ClientId, null, "consent_required", cancellationToken);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count is not 0:
            case ConsentTypes.Explicit when authorizations.Count is not 0 && !request.HasPromptValue(PromptValues.Consent):
                // Same claims the admin cookie would carry, so the access token is a faithful
                // stand-in for the browser session on /api.
                var identity = await BuildTokenIdentityAsync(user.Data.Id.Value, cancellationToken);

                // The cookie principal carries the user name as a single claim; OIDC clients also
                // expect preferred_username next to it.
                identity.SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}");

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                await CompleteTokenIdentityAsync(identity, request.GetScopes(), cancellationToken);

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                var principal = new ClaimsPrincipal(identity);

                // The authorization must exist before its id can be attached to the identity.
                authorization ??= await _authorizationManager.CreateAsync(
                    new OpenIddictAuthorizationDescriptor
                    {
                        Subject = user.Data.Id.Value.ToString(),
                        ApplicationId = await _applicationManager.GetIdAsync(application, cancellationToken),
                        Principal = principal,
                        CreationDate = DateTime.UtcNow,
                        Scopes = [.. identity.GetScopes()],
                        Status = Statuses.Valid,
                        Type = AuthorizationTypes.Permanent
                    },
                    cancellationToken);

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization, cancellationToken));

                await LogValidationAsync(AuthValidationEventType.AuthorizeSucceeded, true, user.Data.Id.Value, request.ClientId, string.Join(" ", request.GetScopes()), null, cancellationToken);
                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                await LogValidationAsync(AuthValidationEventType.AuthorizeDenied, false, user.Data.Id.Value, request.ClientId, null, "consent_required", cancellationToken);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }));

            // In every other case, send the user to the interactive consent page. The full
            // OIDC request is carried in the query string so the page can echo it back and
            // the Accept/Deny actions below complete the authorization with the exact
            // original parameters.
            default:
                return Redirect("/Account/Consent" + Request.QueryString);
        }
    }

    [Authorize, FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the profile of the logged in user.
        var acceptSubject = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new InvalidOperationException("The subject claim cannot be retrieved.");
        var user = await _queryDispatcher.SendAsync(new GetUserByIdQuery(Guid.Parse(acceptSubject)), cancellationToken) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken: cancellationToken) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
          query: (Subject: user.Data.Id.Value.ToString(),
         ApplicationId: await _applicationManager.GetIdAsync(application, cancellationToken),
         Status: Statuses.Valid,
         Type: AuthorizationTypes.Permanent, RequiredScopes: request.GetScopes()),
         cancellationToken: cancellationToken).ToListAsync(cancellationToken);


        // Note: the same check is already made in the other action but is repeated
        // here to ensure a malicious user can't abuse this POST-only endpoint and
        // force it to return a valid response without the external authorization.
        if (authorizations.Count is 0 && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External, cancellationToken))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }));
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = await BuildTokenIdentityAsync(user.Data.Id.Value, cancellationToken);
        identity.SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}");

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        await CompleteTokenIdentityAsync(identity, request.GetScopes(), cancellationToken);

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault();
        var principal = new ClaimsPrincipal(identity);

        // The authorization must exist before its id can be attached to the identity.
        authorization ??= await _authorizationManager.CreateAsync(
                    new OpenIddictAuthorizationDescriptor
                    {
                        Subject = user.Data.Id.Value.ToString(),
                        ApplicationId = await _applicationManager.GetIdAsync(application, cancellationToken),
                        Principal = principal,
                        CreationDate = DateTime.UtcNow,
                        Scopes = [.. identity.GetScopes()],
                        Status = Statuses.Valid,
                        Type = AuthorizationTypes.Permanent
                    },
                    cancellationToken);

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization, cancellationToken));

        foreach (var claim in identity.Claims)
            claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());

        await LogValidationAsync(AuthValidationEventType.AuthorizeSucceeded, true, user.Data.Id.Value, request.ClientId, string.Join(" ", request.GetScopes()), null, cancellationToken);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize, FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    // Notify OpenIddict that the authorization grant has been denied by the resource owner
    // to redirect the user agent to the client application using the appropriate response_mode.
    public async Task<IActionResult> Deny()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(subject, out var parsedUserId) ? parsedUserId : (Guid?)null;

        await LogValidationAsync(AuthValidationEventType.AuthorizeDenied, false, userId, request?.ClientId, null, "user_denied", CancellationToken.None);

        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout"), HttpPost("~/connect/logout"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout([FromQuery] bool confirmed = false)
    {
        // External (relying party) apps hit this endpoint directly to end the session. If there's
        // an active admin cookie and the user hasn't confirmed yet, send them to a confirmation page
        // instead of signing them out immediately - they decide here whether IdentityServer itself
        // should also end its session, not the calling app.
        if (!confirmed && User.Identity?.IsAuthenticated == true)
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            var parameters = new Dictionary<string, string?>
            {
                ["post_logout_redirect_uri"] = request?.PostLogoutRedirectUri,
                ["state"] = request?.State,
                ["client_id"] = request?.ClientId,
                ["id_token_hint"] = request?.IdTokenHint
            };

            var queryString = string.Join('&', parameters
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));

            return Redirect(queryString.Length == 0 ? "/Account/Logout" : $"/Account/Logout?{queryString}");
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var logoutSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var logoutUserId = Guid.TryParse(logoutSubject, out var parsedLogoutUserId) ? parsedLogoutUserId : (Guid?)null;
            await LogValidationAsync(AuthValidationEventType.LogoutSucceeded, true, logoutUserId, null, null, null, CancellationToken.None);

            // Revoke all OpenIddict authorizations and tokens for this user
            if (!string.IsNullOrEmpty(logoutSubject))
            {
                await foreach (var authorization in _authorizationManager.FindBySubjectAsync(logoutSubject))
                {
                    try { await _authorizationManager.TryRevokeAsync(authorization); }
                    catch { /* best-effort revocation */ }
                }

                await foreach (var token in _tokenManager.FindBySubjectAsync(logoutSubject))
                {
                    try { await _tokenManager.TryRevokeAsync(token); }
                    catch { /* best-effort revocation */ }
                }
            }

            // Deactivate the UserSession if a session_token claim is present
            var sessionToken = User.FindFirstValue(OroIdentityServer.Server.Endpoints.AuthEndpoints.SessionTokenClaimType);
            if (!string.IsNullOrEmpty(sessionToken) && logoutUserId.HasValue)
            {
                try
                {
                    var sessions = await _queryDispatcher.SendAsync(
                        new OroIdentityServer.Application.Modules.UserSessions.Queries.GetUserSessionsByUserQuery(logoutUserId.Value),
                        CancellationToken.None);
                    var session = sessions?.FirstOrDefault(s => s.SessionToken == sessionToken);
                    if (session is not null)
                    {
                        await _commandDispatcher.SendAsync(
                            new OroIdentityServer.Application.Modules.UserSessions.Commands.DeactivateUserSessionCommand(session.Id.Value),
                            CancellationToken.None);
                    }
                }
                catch { /* session deactivation must not block logout */ }
            }
        }

        // Delete the local admin cookie created when the user signed in.
        await HttpContext.SignOutAsync(CookieAuthHandlerSetup.AdminScheme);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Userinfo(CancellationToken cancellationToken)
    {
        var subject = User.GetClaim(Claims.Subject) ??
            throw new InvalidOperationException("The subject claim cannot be retrieved.");

        // A client_credentials token authenticates a machine, not a person: its subject is the
        // client id, so there is no user profile this endpoint could return. Answering with an
        // error is the only correct outcome - parsing it as a user id used to surface a 500.
        if (!Guid.TryParse(subject, out var userId))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The access token does not identify a user."
                }));
        }

        var user = await _queryDispatcher.SendAsync(new GetUserByIdQuery(userId), cancellationToken);
        if (user.Data is null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The user details cannot be retrieved."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Data.Id.Value.ToString(),
            [AuthorizationClaimTypes.TenantId] = user.Data.TenantId?.Value.ToString() ?? string.Empty
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.Data.Email!;
        }

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = $"{user.Data.Name} {user.Data.LastName}";
            claims[Claims.GivenName] = user.Data.Name!;
            claims[Claims.FamilyName] = user.Data.LastName!;
            claims[Claims.PreferredUsername] = user.Data.UserName!;

            // Shell consumers (e.g. OroPosSales nav) need the tenant display
            // name without an admin-cookie round-trip: userinfo is fetched
            // with the user's own access token, so it can carry it.
            if (user.Data.TenantId?.Value is { } tenantId && tenantId != Guid.Empty)
            {
                var tenant = await _queryDispatcher.SendAsync(
                    new GetTenantByIdQuery(tenantId), cancellationToken);
                if (!string.IsNullOrWhiteSpace(tenant.Data?.Name))
                {
                    claims["tenant_name"] = tenant.Data.Name;
                }
            }
        }

        if (User.HasScope(Scopes.Roles))
        {
            var roles = await _queryDispatcher.SendAsync(new GetRolesByUserIdQuery(user.Data.Id.Value), cancellationToken);
            claims[Claims.Role] = roles.Data.Select(x => x.Name.Value).ToArray();
        }

        if (User.HasScope(AuthorizationScopes.Permissions))
        {
            var permissions = await _queryDispatcher.SendAsync(new GetPermissionNamesByUserIdQuery(user.Data.Id.Value), cancellationToken);
            claims[AuthorizationClaimTypes.Permission] = (permissions.Data ?? []).ToArray();
        }

        return Ok(claims);
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var user = await _queryDispatcher.SendAsync(new GetUserByIdQuery(Guid.Parse(result.Principal.GetClaim(Claims.Subject))), cancellationToken) ??
         throw new InvalidOperationException("The user details cannot be retrieved.");

            if (user is null)
            {
                await LogValidationAsync(AuthValidationEventType.TokenDenied, false, null, HttpContext.GetOpenIddictServerRequest()?.ClientId, null, "The token is no longer valid.", cancellationToken);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }));
            }

            // Ensure the user is still allowed to sign in.
            if (!await _queryDispatcher.SendAsync(new ValidateUserToLoginQuery(user.Data.Email!), cancellationToken))
            {
                await LogValidationAsync(AuthValidationEventType.TokenDenied, false, user.Data.Id.Value, HttpContext.GetOpenIddictServerRequest()?.ClientId, null, "The user is no longer allowed to sign in.", cancellationToken);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            // The claims are rebuilt from the current user, not carried over from the code/refresh token:
            // a refresh must reflect a role change, a deactivation or a new permission grant made
            // since the original authorization.
            var identity = await BuildTokenIdentityAsync(user.Data.Id.Value, cancellationToken);
            identity.SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}");

            // ClaimsIdentity(IEnumerable<Claim>) copies claims but not OpenIddict's identity properties.
            // Carry the protocol properties over from the code/refresh token so access tokens keep
            // their authorized scopes, resources/audience, authorization link and presenter.
            identity.SetScopes(result.Principal.GetScopes());
            identity.SetResources(result.Principal.GetResources());
            identity.SetAuthorizationId(result.Principal.GetAuthorizationId());
            identity.SetPresenters(result.Principal.GetPresenters());

            var principal = new ClaimsPrincipal(identity);
            foreach (var claim in identity.Claims)
                claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());

            await LogValidationAsync(AuthValidationEventType.TokenIssued, true, user.Data.Id.Value, HttpContext.GetOpenIddictServerRequest()?.ClientId, string.Join(" ", identity.GetScopes()), null, cancellationToken);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // The client exchanges its own credentials for a token: there is no resource owner, so no
        // user claim is added and consent (an authorization-endpoint concern) plays no part.
        if (request.IsClientCredentialsGrantType())
        {
            var clientIdentity = _tokenPrincipalFactory.CreateClientCredentialsIdentity(request.ClientId!);
            await CompleteTokenIdentityAsync(clientIdentity, request.GetScopes(), cancellationToken);

            await LogValidationAsync(AuthValidationEventType.TokenIssued, true, null, request.ClientId, string.Join(" ", clientIdentity.GetScopes()), null, cancellationToken);

            return SignIn(new ClaimsPrincipal(clientIdentity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException($"The specified grant type is not supported: '{request.GrantType}'.");
    }
}