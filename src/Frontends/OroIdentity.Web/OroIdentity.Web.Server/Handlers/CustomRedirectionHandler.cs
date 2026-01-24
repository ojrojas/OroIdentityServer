using Microsoft.AspNetCore.Components;
using OpenIddict.Client;

using static OpenIddict.Client.OpenIddictClientEvents;

namespace OroIdentity.Web.Server.Handlers;

public class CustomRedirectionHandler(
    ILogger<CustomRedirectionHandler> logger, 
    IHttpContextAccessor contextAccessor) : IOpenIddictClientHandler<ApplyRedirectionResponseContext>
{
    public async ValueTask HandleAsync(ApplyRedirectionResponseContext context)
    {
        logger.LogInformation("Custom redirection handler invoked.");
        context.HandleRequest();
    }
}