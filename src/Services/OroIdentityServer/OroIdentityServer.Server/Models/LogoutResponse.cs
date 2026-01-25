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
        var adapter = new AdapterResult(Results.SignOut(
            authenticationSchemes: Schemes,
            properties: Properties));

        await  adapter.ExecuteAsync(context);
    }
}