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
    private readonly string UrlBase = "/connect/token";
    public async Task<HttpResponseMessage> LoginRequest(LoginInputModel loginModel, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context.HttpContext);
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            
            logger.LogInformation("Request login to identityserver");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", loginModel.Email),
                new KeyValuePair<string, string>("password", loginModel.Password),
                new KeyValuePair<string, string>("client_id", configuration["OpenIddict:ClientId"]),
                new KeyValuePair<string, string>("client_secret", configuration["OpenIddict:ClientSecret"])
            });

            var response = await client.PostAsync(UrlBase, content, cancellationToken);
            logger.LogInformation("response to identityserver : {}", JsonSerializer.Serialize(response));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}