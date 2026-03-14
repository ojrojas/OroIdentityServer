using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class RoleClaimsEndpoints
{
    public static RouteGroupBuilder MapRoleClaimsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/roleclaims").WithTags("RoleClaims");

        group.MapGet("get/{id}", GetRoleClaimById);
        group.MapGet("getbyrole/{roleId:guid}", GetRoleClaimsByRoleId);
        group.MapPost("associate", AssociateClaimToRole);
        group.MapPost("associate/bulk", AssociatePermissionsToRole);
        group.MapPut("update", UpdateRoleClaim);
        group.MapDelete("delete/{id}", DeleteRoleClaim);

        return group;
    }

    private static async Task<RoleClaimViewModel?> GetRoleClaimById(
        HttpContext context,
        Guid id,
        [FromServices] IRoleClaimsService service,
        CancellationToken cancellationToken)
    {
        return await service.GetRoleClaimByIdAsync(id, cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>> GetRoleClaimsByRoleId(
        HttpContext context,
        Guid roleId,
        [FromServices] IRoleClaimsService service,
        CancellationToken cancellationToken)
    {
        return await service.GetRoleClaimsByRoleIdAsync(roleId, cancellationToken);
    }

    private static async Task<IResult> AssociateClaimToRole(
        HttpContext context,
        [FromServices] IRoleClaimsService service,
        [FromBody] RoleClaimViewModel payload,
        CancellationToken cancellationToken)
    {
        // payload.RoleClaim.Type and Value expected
        // Use RoleId from payload.Id or payload.RoleClaim ??? Accepting RoleClaimViewModel for simplicity
        await service.AssociateClaimToRoleAsync(payload.Id.Value, payload.ClaimType, payload.ClaimValue, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> AssociatePermissionsToRole(
        HttpContext context,
        [FromServices] IRoleClaimsService service,
        [FromBody] BulkAssociateRequest payload,
        CancellationToken cancellationToken)
    {
        await service.AssociatePermissionsToRoleAsync(payload.RoleId, payload.PermissionIds, cancellationToken);
        return TypedResults.Ok();
    }

    private record BulkAssociateRequest(Guid RoleId, IEnumerable<Guid> PermissionIds);

    private static async Task<IResult> UpdateRoleClaim(
        HttpContext context,
        [FromServices] IRoleClaimsService service,
        [FromBody] RoleClaimViewModel payload,
        CancellationToken cancellationToken)
    {
        await service.UpdateRoleClaimAsync(payload, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> DeleteRoleClaim(
        HttpContext context,
        Guid id,
        [FromServices] IRoleClaimsService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteRoleClaimAsync(id, cancellationToken);
        return TypedResults.Ok();
    }
}
