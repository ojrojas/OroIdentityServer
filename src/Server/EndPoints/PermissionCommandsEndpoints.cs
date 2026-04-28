// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Modules.Permissions.Commands;

namespace OroIdentityServer.Server.Endpoints;

public static class PermissionCommandsEndpoints
{
    public static RouteGroupBuilder MapPermissionCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("permissions");

        api.MapPost(string.Empty, CreatePermission)
            .WithName("CreatePermission");

        api.MapPut(string.Empty, UpdatePermission)
            .WithName("UpdatePermission");

        api.MapDelete("/{id:guid}", DeletePermission)
            .WithName("DeletePermission");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreatePermission(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] CreatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdatePermission(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] UpdatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeletePermission(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePermissionCommand(id), cancellationToken);
        return TypedResults.Ok();
    }
}
