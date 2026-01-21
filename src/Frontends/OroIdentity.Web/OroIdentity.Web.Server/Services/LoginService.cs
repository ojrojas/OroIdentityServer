// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using OpenIddict.Client;
using OroIdentity.Web.Server.Models;

namespace OroIdentity.Web.Server.Services;

public class LoginService(
    ILogger<LoginService> logger,
    IConfiguration configuration,
    HttpClient client,
    IAntiforgery antiforgery,
    OpenIddictClientService openIddictClientService,
    IHttpContextAccessor context) : ILoginService
{
    public HttpClient Client => client;
    private readonly string UrlBase = "/connect/authorize";
    public async Task<HttpResponseMessage> LoginRequest(LoginInputModel loginModel, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context.HttpContext);
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            
            logger.LogInformation("Request login to identityserver");
            // var content = new FormUrlEncodedContent(
            // [
            //     new KeyValuePair<string, string>("grant_type", "authorization_code"),
            //     // new KeyValuePair<string, string>("username", loginModel.Email),
            //     // new KeyValuePair<string, string>("password", loginModel.Password),
            //     new KeyValuePair<string, string>("client_id", configuration["OpenIddict:ClientId"]),
            //     new KeyValuePair<string, string>("client_secret", configuration["OpenIddict:ClientSecret"]),
            //     new KeyValuePair<string, string>("scope", "openid profile email"),
            //     new KeyValuePair<string, string>("response_type", "code"),
            //     new KeyValuePair<string, string>("response_mode", "form_post"),
            //     new KeyValuePair<string, string>("redirect_uri", configuration["IdentityWeb:Url"])
            // ]);

            var result = await openIddictClientService.ChallengeInteractivelyAsync(new()
            {
                CancellationToken = cancellationToken
            });
            
            logger.LogInformation("response to identityserver : {}", JsonSerializer.Serialize(result));

            var response = await openIddictClientService.AuthenticateInteractivelyAsync(new()
            {
                CancellationToken = cancellationToken,
                Properties = result.Properties,
                Nonce = result.Nonce
            });

            if(!string.IsNullOrWhiteSpace(response.TokenResponse.AccessToken))
            {
                logger.LogInformation("Login successful");
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}