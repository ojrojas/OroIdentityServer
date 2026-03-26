// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Collections.Immutable;
using Microsoft.Extensions.Primitives;
using OroBuildingBlocks.ServiceDefaults;
using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
namespace OroIdentityServer.Server.Services;

public class AuthorizationService(
    ILogger<AuthorizationService> logger,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager,
    IConfiguration configuration,
    IApplicationTenantRepository applicationTenantRepository,
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

        var userId = result.Principal!.GetClaim(Claims.Subject);

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

        // Validate that the user has a tenant assigned
        var mappedTenant = await applicationTenantRepository.GetTenantByClientIdAsync(request.ClientId!, cancellationToken);
        if (user?.Data?.TenantId == null)
        {
            return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user does not belong to a tenant."
            }), [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        if (mappedTenant != null && !mappedTenant.Equals(user.Data.TenantId))
        {
            return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The client application is not authorized for the user's tenant."
            }), [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        var authorizations = await authorizationManager.FindAsync(
            subject: user.Data?.Id.Value.ToString(),
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

                identity.SetClaim(Claims.Subject, user?.Data?.Id.Value.ToString())
                        .SetClaim(Claims.Email, user?.Data?.Email)
                        .SetClaim(Claims.Name, $"{user?.Data?.Name} {user?.Data?.LastName}")
                        .SetClaim(Claims.PreferredUsername, user?.Data?.UserName)
                        .SetClaims(Claims.Role,
                            [.. user.Data.Roles.Select(r => r.RoleId.Value.ToString())]);

                // Add tenant claim so downstream services can rely on it
                identity.SetClaim("tenant_id", user?.Data?.TenantId?.Value.ToString());

                identity.SetScopes(request.GetScopes());
                identity.SetResources(
                    await scopeManager.ListResourcesAsync(
                        identity.GetScopes(),
                        cancellationToken)
                        .ToListExtensionsAsync());

                var authorization = authorizations.LastOrDefault();
                authorization ??= await authorizationManager.CreateAsync(
                    identity: identity,
                    subject: user.Data.Id.Value.ToString(),
                    client: (await applicationManager.GetIdAsync(application, cancellationToken))!,
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes(),
                    cancellationToken: cancellationToken);

                var authorizationId = await authorizationManager.GetIdAsync(authorization, cancellationToken);
                identity.SetAuthorizationId(authorizationId);
                identity.SetDestinations(GetDestination.GetDestinations);

                // create session record (ip, country, start) and associate authorization id
                try
                {
                    var ip = requested.Context.Connection.RemoteIpAddress?.ToString() ??
                             requested.Context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown";
                    var country = "unknown";
                    await sender.Send(new CreateSessionCommand(user.Data!.Id, ip, country, user.Data!.TenantId, authorizationId), cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create session record for user {UserId}", user?.Data?.Id);
                }

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

            var userId = result.Principal!.GetClaim(Claims.Subject);
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
            if (!await sender.Send(new ValidateUserToLoginQuery(user?.Data?.Email), cancellationToken))
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
            identity.SetClaim(Claims.Subject, user?.Data?.Id.Value.ToString())
                        .SetClaim(Claims.Email, user?.Data?.Email)
                        .SetClaim(Claims.Name, $"{user?.Data?.Name} {user?.Data?.LastName}")
                        .SetClaim(Claims.PreferredUsername, user?.Data?.UserName)
                        .SetClaims(Claims.Role,
                            [.. user.Data.Roles.Select(r => r.RoleId.Value.ToString())]);

            // Add tenant claim
            identity.SetClaim("tenant_id", user?.Data?.TenantId?.Value.ToString());

            identity.SetDestinations(GetDestination.GetDestinations);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return new LoginResponse(ResultTypes.SignIn, new ClaimsPrincipal(identity), new(), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }
        if (request.IsClientCredentialsGrantType())
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientId);

            var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken)
                ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var displayName = await applicationManager.GetDisplayNameAsync(application, cancellationToken);
            var mappedTenant = await applicationTenantRepository.GetTenantByClientIdAsync(request.ClientId, cancellationToken);

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, request.ClientId)
                    .SetClaim(Claims.Name, displayName ?? request.ClientId)
                    .SetClaim(Claims.PreferredUsername, request.ClientId)
                    .SetClaim(Claims.ClientId, request.ClientId);

            if (mappedTenant is not null)
            {
                identity.SetClaim("tenant_id", mappedTenant.Value.ToString());
            }

            identity.SetScopes(request.GetScopes());
            identity.SetResources(
                await scopeManager.ListResourcesAsync(
                    identity.GetScopes(),
                    cancellationToken)
                    .ToListExtensionsAsync());
            identity.SetDestinations(GetDestination.GetDestinations);

            return new LoginResponse(
                ResultTypes.SignIn,
                new ClaimsPrincipal(identity),
                new(),
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }
        if (request.IsPasswordGrantType())
        {
            // Password grant should not create an authentication cookie; issue tokens only.
            ArgumentNullException.ThrowIfNull(request.Username);
            ArgumentNullException.ThrowIfNull(request.Password);

            // Ensure the user can login
            if (!await sender.Send(new ValidateUserToLoginQuery(request.Username), cancellationToken))
            {
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user cannot log in to the application."
                }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            var user = await sender.Send(new GetUserByEmailQuery(request.Username), cancellationToken);
            var securityUser = await sender.Send(new ValidateUserPasswordQuery(request.Username, request.Password), cancellationToken);

            if (user == null || !securityUser.Data)
            {
                return new LoginResponse(ResultTypes.Forbid, null, Properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            // Build identity for token issuance (no cookie)
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, user?.Data?.Id.Value.ToString())
                    .SetClaim(Claims.Email, user?.Data?.Email)
                    .SetClaim(Claims.Name, $"{user?.Data?.Name} {user?.Data?.LastName}")
                    .SetClaim(Claims.PreferredUsername, user?.Data?.UserName)
                    .SetClaims(Claims.Role, [.. user.Data.Roles.Select(r => r.RoleId.Value.ToString())]);

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestination.GetDestinations);

            // create session record (ip, country, start) but don't create cookie
            try
            {
                var ip = requested.Context.Connection.RemoteIpAddress?.ToString() ??
                         requested.Context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown";
                var country = "unknown";
                await sender.Send(new CreateSessionCommand(user.Data!.Id, ip, country, user?.Data?.TenantId), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create session record for user {UserId}", user?.Data?.Id);
            }

            // Return SignIn result for OpenIddict to issue tokens (no cookie scheme)
            return new LoginResponse(ResultTypes.SignIn, new ClaimsPrincipal(identity), new(), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    public async Task<LoginResponse> LoginAsync(
        SimpleRequest requested,
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

        identity.SetClaim(Claims.Subject, user?.Data?.Id.Value.ToString())
                        .SetClaim(Claims.Email, user?.Data?.Email)
                        .SetClaim(Claims.Name, $"{user?.Data?.Name} {user?.Data?.LastName}")
                        .SetClaim(Claims.PreferredUsername, user?.Data?.UserName)
                        .SetClaims(Claims.Role,
                            [.. user.Data.Roles.Select(r => r.RoleId.Value.ToString())]);

        // Add tenant claim
        identity.SetClaim("tenant_id", user?.Data?.TenantId?.Value.ToString());

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
        var openIdRequest = request.Context.GetOpenIddictServerRequest();
        
        var properties = new AuthenticationProperties();
        if (openIdRequest is not null && !string.IsNullOrEmpty(openIdRequest.PostLogoutRedirectUri))
        {
            properties.RedirectUri = openIdRequest.PostLogoutRedirectUri;
        }
        else
        {
            properties.RedirectUri = "/";
        }

        var response = new LogoutResponse(configuration, properties, [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        ]);
        logger.LogInformation("Logout request success!");
        if (cancellationToken.IsCancellationRequested) throw new InvalidOperationException("Error logout request");
        await Task.CompletedTask;
        return response;
    }

    public async Task<IResult> GetUserInfoAsync(SimpleRequest requested, CancellationToken cancellationToken = default)
    {
        var request = requested.Context.GetOpenIddictServerRequest() ??
           throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await requested.Context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var userId = result.Principal!.GetClaim(Claims.Subject);

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

        var claims = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [Claims.Subject] = userId,
            [Claims.Email] = user?.Data?.Email,
            [Claims.Role] = user?.Data?.Roles?.Select(r => r.RoleId?.Value.ToString()).ToImmutableArray(),
        };
        
        claims[Claims.Name] = $"{user?.Data?.Name} {user?.Data?.LastName}";

        if (user?.Data?.UserName is not null)
        {
            claims[Claims.PreferredUsername] = user?.Data?.Name ?? user?.Data?.UserName;
        }

        return TypedResults.Ok(claims);
    }
}
