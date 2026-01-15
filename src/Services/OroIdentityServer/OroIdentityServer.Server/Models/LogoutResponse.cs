// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Models;

public record LogoutResponse(
        IConfiguration Configuration, AuthenticationProperties? Properties, string Schemes) : IResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest();
        var uriUrl = $"{Configuration["Identity:Url"]}";
        if (request != null && !string.IsNullOrEmpty(request.PostLogoutRedirectUri))
            uriUrl = request.PostLogoutRedirectUri;

        ArgumentNullException.ThrowIfNull(uriUrl);

        _ = Results.SignOut(Properties, [Schemes]) as IResult;
        Results.SignOut();

        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        var adapter = new AdapterResult(Results.Redirect(uriUrl));

        await  adapter.ExecuteAsync(context);
    }
}