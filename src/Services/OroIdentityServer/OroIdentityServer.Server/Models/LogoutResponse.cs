// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Models;

public record LogoutResponse(
        IConfiguration Configuration, AuthenticationProperties? Properties, string[] Schemes) : IResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        var uriUrl = context.Request.PathBase + context.Request.Path + QueryString.Create(
                    context.Request.HasFormContentType ? context.Request.Form : context.Request.Query);
        if(!uriUrl.StartsWith("https") || !uriUrl.StartsWith("http"))
        {
            uriUrl = "/";
        }
       
        ArgumentNullException.ThrowIfNull(uriUrl);

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var adapter = new AdapterResult(Results.SignOut(
            authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties
            {
                RedirectUri =  uriUrl
            }));

        await  adapter.ExecuteAsync(context);
    }
}