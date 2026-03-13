using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class PermissionsEndpoints
{
    public static RouteGroupBuilder MapPermissionsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/permissions").WithTags("Permissions");

        group.MapGet(string.Empty, GetAllPermissions);
        group.MapGet("{id:guid}", GetPermissionById);
        group.MapPost(string.Empty, CreatePermission);
        group.MapPut(string.Empty, UpdatePermission);
        group.MapDelete("{id:guid}", DeletePermissionById);

        return group;
    }

    private static async Task<IResult> CreatePermission(
        HttpContext context,
        [FromServices] IPermissionsService service,
        [FromBody] PermissionViewModel permission,
        CancellationToken cancellationToken)
    {
        await service.CreatePermissionAsync(permission, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> UpdatePermission(
        HttpContext context,
        [FromServices] IPermissionsService service,
        [FromBody] PermissionViewModel permission,
        CancellationToken cancellationToken)
    {
        await service.UpdatePermissionAsync(permission, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task DeletePermissionById(
        HttpContext context,
        Guid id,
        [FromServices] IPermissionsService service,
        CancellationToken cancellationToken)
    {
        await service.DeletePermissionAsync(id, cancellationToken);
    }

    private static async Task<PermissionViewModel?> GetPermissionById(
        HttpContext context,
        Guid id,
        [FromServices] IPermissionsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPermissionByIdAsync(id, cancellationToken);
        return result;
    }

    private static async Task<BaseResponseViewModel<IEnumerable<PermissionViewModel>>> GetAllPermissions(
        HttpContext context,
        [FromServices] IPermissionsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllPermissionsAsync(cancellationToken);
        return result;
    }
}
