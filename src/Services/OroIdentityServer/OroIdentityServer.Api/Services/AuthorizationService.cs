// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Services;

// public class AuthorizationService(
//     IOpenIddictApplicationManager applicationManager,
//     IOpenIddictAuthorizationManager authorizationManager,
//     IOpenIddictScopeManager scopeManager,
//     ILogger<AuthorizationService> logger,
//     IConfiguration configuration,
//     ISender sender) 
// {
//     private readonly IOpenIddictApplicationManager _applicationManager = applicationManager;
//     private readonly IOpenIddictAuthorizationManager _authorizationManager = authorizationManager;
//     private readonly IOpenIddictScopeManager _scopeManager = scopeManager;
//     private readonly ILogger<AuthorizationService> _logger = logger;
//     private readonly IConfiguration _configuration = configuration;
//     private readonly ISender _sender = sender;

//     public async Task<LoginResponse> AuthorizedAsync(SimpleRequest requested, CancellationToken cancellationToken = default)
//     {
//         var request = requested.Context.GetOpenIddictServerRequest() ??
//             throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

//         var result = await requested.Context.AuthenticateAsync();
//         if (result is not { Succeeded: true } ||
//             request.HasPromptValue(PromptValues.Login) || request.MaxAge is 0 ||
//             (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
//              TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
//         {
//             if (request.HasPromptValue(PromptValues.None))
//             {
//                 return new LoginResponse(ResultTypes.Forbid, null, properties: new AuthenticationProperties(new Dictionary<string, string?>
//                 {
//                     [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
//                     [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
//                            "The user is not logged in."
//                 }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
//             }

//             return new LoginResponse(ResultTypes.Challenge, null, new AuthenticationProperties
//             {
//                 RedirectUri = requested.Context.Request.PathBase + requested.Context.Request.Path + QueryString.Create(
//                     requested.Context.Request.HasFormContentType ? requested.Context.Request.Form : requested.Context.Request.Query)
//             }, []);
//         }

//         var user = await _sender.Send(new GetUserByIdQuery(result.Principal!.GetClaim(Claims.Subject)!), cancellationToken);
//         if (user == null)
//         {
//             return new LoginResponse(ResultTypes.Challenge, null, new AuthenticationProperties
//             {
//                 RedirectUri = requested.Context.Request.PathBase + requested.Context.Request.Path + QueryString.Create(
//                   requested.Context.Request.HasFormContentType ? requested.Context.Request.Form : requested.Context.Request.Query)
//             }, []);
//         }

//         var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
//             throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

//         var authorizations = await _authorizationManager.FindAsync(
//             subject: user.Id,
//             client: await _applicationManager.GetIdAsync(application),
//             status: Statuses.Valid,
//             type: AuthorizationTypes.Permanent,
//             scopes: request.GetScopes()).ToListExtensionsAsync();

//         switch (await _applicationManager.GetConsentTypeAsync(application))
//         {
//             case ConsentTypes.External when authorizations.Count is 0:
//                 return new LoginResponse(ResultTypes.Forbid, null, properties: new AuthenticationProperties(new Dictionary<string, string?>
//                 {
//                     [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
//                     [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
//                             "The logged in user is not allowed to access this client application."
//                 }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

//             case ConsentTypes.Implicit:
//             case ConsentTypes.External when authorizations.Count is not 0:
//             case ConsentTypes.Explicit when !request.HasPromptValue(PromptValues.Consent):
//             case ConsentTypes.Explicit when authorizations.Count is not 0 && !request.HasPromptValue(PromptValues.Consent):
//                 var identity = new ClaimsIdentity(
//                     authenticationType: TokenValidationParameters.DefaultAuthenticationType,
//                     nameType: Claims.Name,
//                     roleType: Claims.Role);

//                 identity.SetClaim(Claims.Subject, user.Id)
//                         .SetClaim(Claims.Email, user.Email)
//                         .SetClaim(Claims.Name, user.UserName)
//                         .SetClaim(Claims.PreferredUsername, user.UserName)
//                         .SetClaims(Claims.Role, user.Roles);

//                 identity.SetScopes(request.GetScopes());
//                 identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListExtensionsAsync());

//                 var authorization = authorizations.LastOrDefault();
//                 authorization ??= await _authorizationManager.CreateAsync(
//                     identity: identity,
//                     subject: user.Id,
//                     client: (await _applicationManager.GetIdAsync(application))!,
//                     type: AuthorizationTypes.Permanent,
//                     scopes: identity.GetScopes());

//                 identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
//                 identity.SetDestinations(GetDestination.GetDestinations);

//                 return new LoginResponse(ResultTypes.SignIn, new ClaimsPrincipal(identity), new(), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

//             default:
//                 return new LoginResponse(ResultTypes.BadRequest, null, null, []);
//         }
//     }

//     public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
//     {
//         _logger.LogInformation("login user application request");
//         var user = await _sender.Send(new GetUserByEmailQuery(request.UserName), cancellationToken);
//         if (user == null || user.Data.SecurityUser.PasswordHash != request.Password)
//         {
//             var properties = new AuthenticationProperties(new Dictionary<string, string?>
//             {
//                 [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
//                 [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
//                                 "The username/password couple is invalid."
//             });

//             return new LoginResponse(ResultTypes.Forbid, new(), properties, [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
//         }

//         _logger.LogInformation("Credentials user validate successful");
//         var identity = new ClaimsIdentity(
//         authenticationType: TokenValidationParameters.DefaultAuthenticationType,
//         nameType: Claims.Name,
//         roleType: Claims.Role);

//         identity.SetClaim(Claims.Subject, user.Data.Id)
//                 .SetClaim(Claims.Email, user.Data.Email)
//                 .SetClaim(Claims.Name, user.Data.UserName)
//                 .SetClaim(Claims.PreferredUsername, user.Data.UserName)
//                 .SetClaims(Claims.Role, user.Data.Roles);

//         identity.SetScopes(new[]
//         {
//             Scopes.OpenId,
//             Scopes.Email,
//             Scopes.Profile,
//             Scopes.Roles
//         });

//         identity.SetDestinations(GetDestination.GetDestinations);
//         _logger.LogInformation("Login user application successful");
//         return new LoginResponse(ResultTypes.SignIn, new ClaimsPrincipal(identity), new(), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
//     }
// }