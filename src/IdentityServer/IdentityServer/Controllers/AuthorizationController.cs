using System.Security.Claims;
using IdentityServer.Server.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OroIdentityServer.Server.Authentication;
using OroIdentityServer.Server.Helpers;
using static OpenIddict.Abstractions.OpenIddictConstants;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.Users.Queries;
using OroIdentityServer.Application.Modules.Roles.Queries;

namespace OroIdentityServer.Server.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IQueryDispatcher _queryDispatcher;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        IQueryDispatcher queryDispatcher)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _queryDispatcher = queryDispatcher;
    }

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
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            subject: user.Data.Id.Value.ToString(),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case ConsentTypes.External when authorizations.Count is 0:
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
                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                var roles = await _queryDispatcher.SendAsync(new GetRolesByUserIdQuery(user.Data.Id.Value), cancellationToken);

                // Add the claims that will be persisted in the tokens.
                identity.SetClaim(Claims.Subject, user.Data.Id.Value.ToString())
                        .SetClaim(Claims.Email, user.Data.Email)
                        .SetClaim(Claims.Name, $"{user.Data.Name} {user.Data.LastName}")
                        .SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}")
                        .SetClaims(Claims.Role, [.. roles.Data.Select(x=> x.Name)]);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                identity.SetScopes(request.GetScopes());
                identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: user.Data.Id.Value.ToString(),
                    client: (await _applicationManager.GetIdAsync(application))!,
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                var principal = new ClaimsPrincipal(identity);
foreach (var claim in identity.Claims)
    claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }));

            // In every other case, render the consent form.
            default:
                return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                    Scope = request.Scope
                });
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
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            subject: user.Data.Id.Value.ToString(),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Note: the same check is already made in the other action but is repeated
        // here to ensure a malicious user can't abuse this POST-only endpoint and
        // force it to return a valid response without the external authorization.
        if (authorizations.Count is 0 && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
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
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        var roles = await _queryDispatcher.SendAsync(new GetRolesByUserIdQuery(user.Data.Id.Value), cancellationToken);

        // Add the claims that will be persisted in the tokens.
        identity.SetClaim(Claims.Subject, user.Data.Id.Value.ToString())
                .SetClaim(Claims.Email, user.Data.Email)
                .SetClaim(Claims.Name, $"{user.Data.Name} {user.Data.LastName}")
                .SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}")
                .SetClaims(Claims.Role, [.. roles.Data.Select(x=> x.Name)]);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
            identity: identity,
            subject: user.Data.Id.Value.ToString(),
            client: (await _applicationManager.GetIdAsync(application))!,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        var principal = new ClaimsPrincipal(identity);
foreach (var claim in identity.Claims)
    claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize, FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    // Notify OpenIddict that the authorization grant has been denied by the resource owner
    // to redirect the user agent to the client application using the appropriate response_mode.
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    [HttpGet("~/connect/logout"), HttpPost("~/connect/logout"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
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

        var user = await _queryDispatcher.SendAsync(new GetUserByIdQuery(Guid.Parse(subject)), cancellationToken) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Data.Id.Value.ToString()
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
        }

        if (User.HasScope(Scopes.Roles))
        {
            var roles = await _queryDispatcher.SendAsync(new GetRolesByUserIdQuery(user.Data.Id.Value), cancellationToken);
            claims[Claims.Role] = roles.Data.Select(x => x.Name).ToArray();
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
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            var roles = await _queryDispatcher.SendAsync(new GetRolesByUserIdQuery(user.Data.Id.Value), cancellationToken);

            // Override the user claims present in the principal in case they
            // changed since the authorization code/refresh token was issued.
            identity.SetClaim(Claims.Subject, user.Data.Id.Value.ToString())
                    .SetClaim(Claims.Email, user.Data.Email)
                    .SetClaim(Claims.Name, $"{user.Data.Name} {user.Data.LastName}")
                    .SetClaim(Claims.PreferredUsername, $"{user.Data.Name} {user.Data.LastName}")
                    .SetClaims(Claims.Role, [.. roles.Data.Select(x=> x.Name)]);

            var principal = new ClaimsPrincipal(identity);
foreach (var claim in identity.Claims)
    claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }
}