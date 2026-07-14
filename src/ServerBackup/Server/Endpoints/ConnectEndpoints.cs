// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore;
using OpenIddict.Server.AspNetCore;
using OroIdentityServer.Server.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentityServer.Server.Endpoints;

public static class ConnectEndpoints
{
    public static IEndpointRouteBuilder MapConnectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/connect").WithTags("OIDC");

        group.MapMethods("authorize", ["GET", "POST"], HandleAuthorize);
        group.MapMethods("logout", ["GET", "POST"], HandleEndSession);
        group.MapPost("token", HandleToken);
        group.MapMethods("userinfo", ["GET", "POST"], HandleUserInfo);

        return app;
    }

    private static async Task<IResult> HandleAuthorize(HttpContext http)
    {
        var request = http.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenID Connect request not found.");

        var auth = await http.AuthenticateAsync(CookieAuthHandlerSetup.AdminScheme);
        if (!auth.Succeeded)
        {
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = http.Request.Path + http.Request.QueryString },
                [CookieAuthHandlerSetup.AdminScheme]);
        }

        var subject = auth.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(Claims.Subject, subject).SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        identity.AddClaim(new Claim(Claims.Name, auth.Principal.Identity?.Name ?? subject)
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        var email = auth.Principal.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrEmpty(email))
            identity.AddClaim(new Claim(Claims.Email, email).SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        foreach (var role in auth.Principal.FindAll(ClaimTypes.Role))
            identity.AddClaim(new Claim(Claims.Role, role.Value).SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());

        return Results.SignIn(principal, properties: null, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> HandleToken(HttpContext http)
    {
        var request = http.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenID Connect request not found.");

        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
        {
            return Results.BadRequest(new { error = "unsupported_grant_type" });
        }

        var auth = await http.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = auth.Principal
            ?? throw new InvalidOperationException("Cannot extract principal from token.");

        return Results.SignIn(principal, properties: null, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static Task<IResult> HandleUserInfo(HttpContext http)
    {
        var user = http.User;
        return Task.FromResult<IResult>(Results.Ok(new
        {
            sub = user.FindFirstValue(Claims.Subject),
            name = user.Identity?.Name,
            email = user.FindFirstValue(Claims.Email),
            roles = user.FindAll(Claims.Role).Select(c => c.Value).ToArray()
        }));
    }

    private static async Task<IResult> HandleEndSession(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthHandlerSetup.AdminScheme);
        return Results.SignOut(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
