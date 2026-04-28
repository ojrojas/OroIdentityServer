// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Modules.Permissions.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static class PermissionQueriesEndpoints
{
    public static RouteGroupBuilder MapPermissionQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("permissions");

        api.MapGet(string.Empty, GetAllPermissions)
           .WithName("GetAllPermissions");

        api.MapGet("/{id:guid}", GetPermissionById)
           .WithName("GetPermissionById");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok<GetPermissionsQueryResponse>, ProblemHttpResult>> GetAllPermissions(
        HttpContext context,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPermissionsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<GetPermissionByIdQueryResponse>, NotFound>> GetPermissionById(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPermissionByIdQuery(id), cancellationToken);
        return result.Data != null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }
}
