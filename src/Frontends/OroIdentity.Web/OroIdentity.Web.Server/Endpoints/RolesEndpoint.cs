using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class RolesEndpoints
{
    public static RouteGroupBuilder MapRolesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/roles")
            .WithTags("Roles");


        group.MapGet(string.Empty, GetAllRoles);
        group.MapGet("/{name}", GetRoleByName);
        group.MapGet("/{roleId: guid}", GetRoleById);

        group.MapDelete("/{roleId: guid}", DeleteRoleById);
        group.MapPost(string.Empty, CreateRole);
        group.MapPut(string.Empty, UpdateRole);

        return group;
    }

    private static async Task DeleteRoleById(
        HttpContext context,
        Guid roleId,
        [FromServices] IRolesService service,
        CancellationToken cancellationToken
        )
    {
        await service.DeleteRoleAsync(roleId.ToString(), cancellationToken);
    }

    private static async Task<IResult> UpdateRole(
        HttpContext context,
        [FromServices] IRolesService service,
        [FromBody] RoleViewModel role,
        CancellationToken cancellationToken)
    {
        await service.UpdateRoleAsync(role, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> CreateRole(
        HttpContext context,
        [FromServices] IRolesService service,
        [FromBody] RoleViewModel role,
        CancellationToken cancellationToken)
    {
        await service.CreateRoleAsync(role, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<RoleViewModel?> GetRoleById(
        HttpContext context,
        Guid roleId,
        [FromServices] IRolesService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetRoleByIdAsync(roleId.ToString(), cancellationToken);
    }

    private static async Task<RoleViewModel?> GetRoleByName(
        HttpContext context,
        string name,
        [FromServices] IRolesService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetRoleByNameAsync(name, cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IEnumerable<RoleViewModel>>?> GetAllRoles(
        HttpContext context,
        [FromServices] IRolesService service,
        CancellationToken cancellationToken
        )
    {
       return await service.GetAllRolesAsync(cancellationToken);
    }
}