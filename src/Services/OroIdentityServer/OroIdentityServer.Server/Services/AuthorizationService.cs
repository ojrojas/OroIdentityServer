// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Collections.Immutable;
using Microsoft.Extensions.Primitives;
namespace OroIdentityServer.Services.OroIdentityServer.Server.Services;

public class AuthorizationService(
    ILogger<AuthorizationService> logger,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager,
    IConfiguration configuration,
    ISender sender) : IAuthorizationService
{
    public async Task<LoginResponse> AuthorizedAsync(SimpleRequest requested, CancellationToken cancellationToken = default)
    {
        var request = requested.Context.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await requested.Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result is not { Succeeded: true } ||
            request.HasPromptValue(PromptValues.Login) || request.MaxAge is 0 ||
            (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
             TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                           "The user is not logged in."
                }), [CookieAuthenticationDefaults.AuthenticationScheme]);
            }

            if (!result.Succeeded)
            {
                var prompt = string.Join(" ", request.GetPromptValues().Remove(PromptValues.Login));

                var parameters = requested.Context.Request.HasFormContentType ?
                  requested.Context.Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                  requested.Context.Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                var url = requested.Context.Request.PathBase + requested.Context.Request.Path + QueryString.Create(parameters);

                return new LoginResponse(ResultTypes.Challenge, null,
                 new AuthenticationProperties
                 {
                     RedirectUri = url,
                 },
                 [CookieAuthenticationDefaults.AuthenticationScheme]);
            }
        }

        var userId = result.Principal!.GetClaim(Claims.Subject)!.Trim('"');

        ArgumentNullException.ThrowIfNull(userId);

        var user = await sender.Send(new GetUserByIdQuery(new(Guid.Parse(userId))), cancellationToken);
        if (user == null)
        {
            return new LoginResponse(ResultTypes.Challenge, null, new AuthenticationProperties
            {
                RedirectUri = requested.Context.Request.PathBase + requested.Context.Request.Path + QueryString.Create(
                  requested.Context.Request.HasFormContentType ? requested.Context.Request.Form : requested.Context.Request.Query)
            }, [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken: cancellationToken) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var authorizations = await authorizationManager.FindAsync(
            subject: user.Data?.Id.ToString(),
            client: await applicationManager.GetIdAsync(application, cancellationToken),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes(),
            cancellationToken: cancellationToken)
            .ToListExtensionsAsync();

        switch (await applicationManager.GetConsentTypeAsync(
            application,
            cancellationToken))
        {
            case ConsentTypes.External when authorizations.Count is 0:
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                }), [CookieAuthenticationDefaults.AuthenticationScheme]);

            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count is not 0:
            case ConsentTypes.Explicit when !request.HasPromptValue(PromptValues.Consent):
            case ConsentTypes.Explicit when authorizations.Count is not 0 && !request.HasPromptValue(PromptValues.Consent):
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                identity.SetClaim(Claims.Subject, user.Data.Id.Value)
                        .SetClaim(Claims.Email, user.Data.Email)
                        .SetClaim(Claims.Name, user.Data.UserName)
                        .SetClaim(Claims.PreferredUsername, user.Data.UserName)
                        .SetClaims(
                            "Roles",
                            user.Data.Roles.Select(r => r.RoleId.ToString()).ToImmutableArray());

                identity.SetScopes(request.GetScopes());
                identity.SetResources(
                    await scopeManager.ListResourcesAsync(
                        identity.GetScopes(),
                        cancellationToken)
                        .ToListExtensionsAsync());

                var authorization = authorizations.LastOrDefault();
                authorization ??= await authorizationManager.CreateAsync(
                    identity: identity,
                    subject: user.Data.Id.ToString(),
                    client: (await applicationManager.GetIdAsync(application, cancellationToken))!,
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes(),
                    cancellationToken: cancellationToken);

                identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));
                identity.SetDestinations(GetDestination.GetDestinations);

                return new LoginResponse(
                    ResultTypes.SignIn,
                    new ClaimsPrincipal(identity),
                    new(),
                    [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

            default:
                return new LoginResponse(ResultTypes.BadRequest, null, null, []);
        }
    }

    public async Task<LoginResponse> GetTokenAsync(SimpleRequest requested, CancellationToken cancellationToken)
    {
        var request = requested.Context.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await requested.Context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result is not { Succeeded: true })
            {
                throw new InvalidOperationException("The token is no longer valid.");
            }
            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var valueId = result.Principal.GetClaim(Claims.Subject);
            ArgumentNullException.ThrowIfNull(valueId);

            var userId = result.Principal!.GetClaim(Claims.Subject)!.Trim('"');
            var user = await sender.Send(new GetUserByIdQuery(new(Guid.Parse(userId))), cancellationToken);
            if (user is null)
            {
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }), [CookieAuthenticationDefaults.AuthenticationScheme]);
            }

            // Ensure the user is still allowed to sign in.
            if (!await sender.Send(new ValidateUserToLoginQuery(user.Data.Email), cancellationToken))
            {
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }), [CookieAuthenticationDefaults.AuthenticationScheme]);
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Override the user claims present in the principal in case they
            // changed since the authorization code/refresh token was issued.
            identity.SetClaim(Claims.Subject, user.Data.Id.Value)
                .SetClaim(Claims.Email, user.Data.Email)
                .SetClaim(Claims.Name, user.Data.UserName)
                .SetClaim(Claims.PreferredUsername, user.Data.UserName)
                // .SetClaims(Claims.Role, user.Data.Roles)
                ;

            identity.SetDestinations(GetDestination.GetDestinations);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return new LoginResponse(ResultTypes.SignIn, new ClaimsPrincipal(identity), new(), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }
        if (request.IsPasswordGrantType())
        {
            ArgumentNullException.ThrowIfNull(request.Username);
            ArgumentNullException.ThrowIfNull(request.Password);

            var loginRequest = new LoginRequest(request.Username, request.Password, false);
            return await LoginAsync(loginRequest, cancellationToken);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("login user application request");

        var validateUserCanLogin = await sender.Send(new ValidateUserToLoginQuery(request.UserName), cancellationToken);

        if (!validateUserCanLogin)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The user cannot log in to the application."
            });

            return new LoginResponse(ResultTypes.Forbid, new(), properties, [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        var user = await sender.Send(new GetUserByEmailQuery(request.UserName), cancellationToken);

        var securityUser = await sender.Send(new ValidateUserPasswordQuery(request.UserName, request.Password), cancellationToken);

        if (user == null || !securityUser.Data)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The username/password couple is invalid."
            });

            return new LoginResponse(ResultTypes.Unauthorized, new(), properties, [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        logger.LogInformation("Credentials user validate successful");
        var identity = new ClaimsIdentity(
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.Data.Id.Value)
                .SetClaim(Claims.Email, user.Data.Email)
                .SetClaim(Claims.Name, user.Data.UserName)
                .SetClaim(Claims.PreferredUsername, user.Data.UserName)
                // .SetClaims(Claims.Role, user.Data.Roles)
                ;

        identity.SetScopes(new[]
        {
            Scopes.OpenId,
            Scopes.Email,
            Scopes.Profile,
            Scopes.Roles
        });

        identity.SetDestinations(GetDestination.GetDestinations);
        logger.LogInformation("Login user application successful");
        return new LoginResponse(
             ResultTypes.SignIn,
             new ClaimsPrincipal(identity),
             new(),
             [CookieAuthenticationDefaults.AuthenticationScheme]);

    }

    public async Task<LogoutResponse> LogoutAsync(SimpleRequest request, CancellationToken cancellationToken = default)
    {
        var response = new LogoutResponse(configuration, null, [CookieAuthenticationDefaults.AuthenticationScheme]);
        logger.LogInformation("Logout request success!");
        if (cancellationToken.IsCancellationRequested) throw new InvalidOperationException("Error logout request");
        await Task.CompletedTask;
        return response;
    }
}