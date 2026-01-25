using OpenIddict.Client;

using static OpenIddict.Client.OpenIddictClientEvents;

namespace OroIdentity.Web.Server.Handlers;

public class CustomRedirectionHandler(
    ILogger<CustomRedirectionHandler> logger) : IOpenIddictClientHandler<ApplyRedirectionResponseContext>
{
    public async ValueTask HandleAsync(ApplyRedirectionResponseContext context)
    {
        if(context.IsRequestHandled)
        {
            return;
        }
        logger.LogInformation("Custom redirection handler invoked.");
        ArgumentNullException.ThrowIfNull(context.RequestUri);
        // Do not interfere with the default OpenIddict redirection handling here.
        // If custom behavior is required, apply the response and call context.HandleRequest().
        context.HandleRequest();
    }
}