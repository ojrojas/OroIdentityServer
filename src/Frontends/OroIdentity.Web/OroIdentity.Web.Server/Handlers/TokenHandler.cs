
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Client.OpenIddictClientModels;

namespace OroIdentity.Web.Server.Handlers;

public class TokenHandler(IHttpContextAccessor httpContextAccessor) : 
    DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            throw new Exception("HttpContext not available");
        }

        var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync(
            OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}