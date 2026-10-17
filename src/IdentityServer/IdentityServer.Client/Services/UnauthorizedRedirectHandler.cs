using Microsoft.AspNetCore.Components;

namespace IdentityServer.Client.Services;

/// <summary>
/// Turns the bare <c>401 unauthorized</c> the /api endpoints answer when the session cookie is
/// gone into a navigation to the login page. Without it the SPA stays on the current route and
/// renders an empty shell, because a fetch/XHR response is never a navigation.
/// </summary>
public sealed class UnauthorizedRedirectHandler(NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // Guarded against the login page itself: once the user is there, /api is not called, and
        // navigating again would only discard the reason/ReturnUrl the server just sent.
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            && !navigation.Uri.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase))
        {
            var location = response.Headers.Location?.ToString();
            navigation.NavigateTo(string.IsNullOrWhiteSpace(location) ? "/Account/Login?reason=expired" : location);
        }

        return response;
    }
}