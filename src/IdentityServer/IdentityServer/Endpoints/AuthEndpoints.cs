// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using IdentityServer.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OroIdentityServer.Server.Authentication;

namespace OroIdentityServer.Server.Endpoints;

public static class AuthEndpoints
{
    public sealed record LoginRequest(string LoginIdentifier, string Password, string? ReturnUrl);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            [FromServices]HttpContext http,
            [FromForm]LoginInputModel loginInput,
            [FromServices]AdminPasswordSignInService signInService,
            CancellationToken ct) =>
        {
            var principal = await signInService.SignInAsync(loginInput.LoginIdentifier, loginInput.Password, ct);
            if (principal is null)
            {
                return Results.Redirect("/login?error=invalid");
            }

            await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);

            var target = string.IsNullOrWhiteSpace(loginInput.ReturnUrl) ? "/" : loginInput.ReturnUrl;
            return Results.Redirect(target);
        }).DisableAntiforgery();

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthHandlerSetup.AdminScheme);
            return Results.Redirect("/login");
        }).DisableAntiforgery();

        return app;
    }


}
