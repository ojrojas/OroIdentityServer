// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class RoleCommandsEndpoints
{
    public static RouteGroupBuilder MapRoleCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/roles");

        api.MapPost("/create", CreateRole)
            .WithName("CreateRole");

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateRole(
        HttpContext context,
        [FromBody] CreateRoleCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }
}