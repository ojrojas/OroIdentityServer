using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using OroIdentity.Web.Server.Models;

namespace OroIdentity.Web.Server.Services;

public class LoginService(
    ILogger<LoginService> logger,
    HttpClient client,
    IAntiforgery antiforgery,
    IHttpContextAccessor context) : ILoginService
{
    private readonly string UrlBase = "account/login";
    public async Task<HttpResponseMessage> LoginRequest(LoginInputModel loginModel)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(context.HttpContext);
            antiforgery?.ValidateRequestAsync(context.HttpContext);
            logger.LogInformation("Request login to identityserver");
            var requestLogin = new
            {
                UserName= loginModel.Email,
                loginModel.Password
            };
            var response = await client.PostAsJsonAsync(UrlBase, requestLogin);
            logger.LogInformation("response to identityserver : {}", JsonSerializer.Serialize(response));
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}