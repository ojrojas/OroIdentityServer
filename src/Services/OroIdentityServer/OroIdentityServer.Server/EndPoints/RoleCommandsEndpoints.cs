// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;

public static class RoleCommandsEndpoints
{
    public static RouteGroupBuilder MapRoleCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("roles");

        api.MapPost(string.Empty, CreateRole)
            .WithName("CreateRole");

        api.MapPut(string.Empty, UpdateRole)
            .WithName("UpdateRole");

        api.MapDelete("{id:guid}", DeleteRole)
            .WithName("DeleteRole");

        api.RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        });
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

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateRole(
        HttpContext context,
        [FromBody] UpdateRoleCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> DeleteRole(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteRoleCommand(new(id)), cancellationToken);
        return TypedResults.Ok();
    }
}