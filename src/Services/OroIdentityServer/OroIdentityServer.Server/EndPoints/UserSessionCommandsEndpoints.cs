// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class UserSessionCommandsEndpoints
{
    public static RouteGroupBuilder MapUserSessionCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("users");

        api.MapPost("/{id:guid}/sessions", CreateUserSession)
            .WithName("CreateUserSession");

        api.MapPatch("/sessions/{id:guid}", UpdateUserSession)
            .WithName("UpdateUserSession");

        api.MapDelete("/sessions/{id:guid}", DeactivateUserSession)
            .WithName("DeactivateUserSession");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateUserSession(
        HttpContext context,
        [FromRoute] Guid id,
        [FromBody] CreateUserSessionCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        // Ensure path id matches body user id if provided
        if (request.UserId == null || request.UserId.Value != id)
        {
            request = request with { UserId = new UserId(id) };
        }

        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateUserSession(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUserSessionCommand(new UserSessionId(id)), cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeactivateUserSession(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivateUserSessionCommand(new UserSessionId(id)), cancellationToken);
        return TypedResults.Ok();
    }
}
