// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class AuthorizedEndpoints
{
    public static RouteGroupBuilder MapAuthorizeEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup(string.Empty);

        api.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], AuthorizeApp);
        api.MapPost("/connect/token", GetToken);
        api.MapGet("/connect/logout", Logout);
        api.MapPost("/account/login", Login);

        return api;
    }

    private static async ValueTask<IResult> Login(
        HttpContext context,
        [FromServices] IAuthorizationService service,
        [FromBody]LoginRequest loginRequest,
        CancellationToken cancellationToken)
    {
        return await service.LoginAsync(loginRequest, cancellationToken);
    }

    [IgnoreAntiforgeryToken]
    private static async ValueTask<IResult> Logout(
           HttpContext context,
          [FromServices] IAuthorizationService service,
           CancellationToken cancellationToken)
    {
        return await service.LogoutAsync(new(context), cancellationToken);
    }

    private static async ValueTask<LoginResponse> GetToken(
        HttpContext context,
        [FromServices] IAuthorizationService service,
        CancellationToken cancellationToken)
    {
        return await service.GetTokenAsync(new(context), cancellationToken);
    }

    [IgnoreAntiforgeryToken]
    private static async ValueTask<LoginResponse> AuthorizeApp(
          HttpContext context,
          [FromServices] IAuthorizationService service,
          CancellationToken cancellationToken)
    {
        return await service.AuthorizedAsync(new(context), cancellationToken);
    }
}