using System.Text.Json;
using System.Text.Json.Serialization;
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
        logger.LogInformation("Custom redirection handler completed.");
        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Log the authentication type if available (e.g., "Federation")
        try
        {
            string? authType = null;

            // Try to reflectively find an HttpContext or Transaction.HttpContext and read the User.Identity.AuthenticationType
            var ctxType = context.GetType();
            var httpProp = ctxType.GetProperty("HttpContext");
            object? httpObj = null;

            if (httpProp != null)
            {
                httpObj = httpProp.GetValue(context);
            }
            else
            {
                var txProp = ctxType.GetProperty("Transaction");
                var txObj = txProp?.GetValue(context);
                if (txObj != null)
                {
                    var txHttpProp = txObj.GetType().GetProperty("HttpContext");
                    httpObj = txHttpProp?.GetValue(txObj);
                }
            }

            if (httpObj != null)
            {
                var userProp = httpObj.GetType().GetProperty("User");
                var userObj = userProp?.GetValue(httpObj);
                if (userObj is System.Security.Claims.ClaimsPrincipal cp)
                {
                    authType = cp.Identity?.AuthenticationType;
                }
                else if (userObj != null)
                {
                    var idProp = userObj.GetType().GetProperty("Identity");
                    var idObj = idProp?.GetValue(userObj);
                    var atProp = idObj?.GetType().GetProperty("AuthenticationType");
                    authType = atProp?.GetValue(idObj) as string;
                }
            }

            logger.LogInformation("Authentication type present: {authType}", authType ?? "(none)");
        }
        catch
        {
            // Ignore logging failures
        }

        logger.LogInformation("Redirecting to: {context}", JsonSerializer.Serialize(context, jsonOptions));
    }
}