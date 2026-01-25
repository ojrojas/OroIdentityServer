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
    IAntiforgery antiforgery,
    OpenIddictClientService openIddictClientService,
    IHttpContextAccessor context) : ILoginService
{
    public async Task<HttpResponseMessage> LoginRequest(LoginInputModel loginModel, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context.HttpContext);
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            
            logger.LogInformation("Request login to identityserver");

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

            logger.LogInformation("Authentication response: {}", JsonSerializer.Serialize(response));

            if (!string.IsNullOrWhiteSpace(response.TokenResponse?.AccessToken))
            {
                logger.LogInformation("Login successful, access token received");
                // Aquí puedes agregar lógica para manejar el token, como guardarlo en sesión o cookies
            }
            else
            {
                logger.LogWarning("No access token received in response");
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}