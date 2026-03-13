
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OpenIddict.Client.AspNetCore;
using OroIdentity.Web.Client.Constants;

namespace OroIdentity.Web.Server.Handlers;

internal sealed class TokenHandler(
    IHttpContextAccessor httpContextAccessor,
    ProtectedSessionStorage storage,
    IHttpClientFactory httpClientFactory) :
    DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            throw new Exception("HttpContext not available");
        }

        var accessToken = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentNullException("The access token couldn't be found in the request options.");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshTokenAsync();

            if (string.IsNullOrWhiteSpace(refreshed))
                return response;

            request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshed);

            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async Task<string> GetTokenAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        // 1) Prefer tokens stored in ProtectedSessionStorage (used by the UI components)
        try
        {
            var sessionToken = await storage.GetAsync<string>("access_token");
            if (sessionToken.Success && !string.IsNullOrWhiteSpace(sessionToken.Value))
            {
                return sessionToken.Value;
            }
        }
        catch
        {
            // ignore storage failures (circuit may not exist for some requests)
        }

        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
            return string.Empty;

        // 2) Try to get token from the OpenIddict client authentication result (explicit scheme)
        try
        {
            // common fallback names
            var token = await ctx.GetTokenAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            // check cookie authentication scheme
            token = await ctx.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            // last attempt: default scheme + backchannel token name
            token = await ctx.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);
            if (!string.IsNullOrWhiteSpace(token))
                return token;
        }
        catch
        {
            // ignore and return empty
        }

        return string.Empty;
    }

    public async Task<string> TryRefreshTokenAsync()
    {
        var refresh = await storage.GetAsync<string>("refresh_token");

        if (!refresh.Success || string.IsNullOrWhiteSpace(refresh.Value))
            return string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refresh.Value
            })
        };

        var client = httpClientFactory.CreateClient(OroIdentityWebConstants.OroIdentityServerApis);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();

        await storage.SetAsync("access_token", payload!.AccessToken);
        await storage.SetAsync("refresh_token", payload.RefreshToken);

        return payload.AccessToken;
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

