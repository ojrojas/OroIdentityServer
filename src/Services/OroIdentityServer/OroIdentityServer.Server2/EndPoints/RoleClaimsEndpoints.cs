// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using OroIdentityServer.Services.OroIdentityServer.Core.Models;

namespace OroIdentityServer.OroIdentityServer.Api.EndPoints;

public static class RoleClaimsEndpoints
{
    public static RouteGroupBuilder MapRoleClaimsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/roleclaims");

        api.MapGet("/get/{id}", GetRoleClaimById)
            .WithName("GetRoleClaimById");

        api.MapGet("/getbyrole/{roleId}", GetRoleClaimsByRoleId)
            .WithName("GetRoleClaimsByRoleId");

        api.MapPost("/associate", AssociateClaimToRole)
            .WithName("AssociateClaimToRole");

        api.MapPut("/update", UpdateRoleClaim)
            .WithName("UpdateRoleClaim");

        api.MapDelete("/delete/{id}", DeleteRoleClaim)
            .WithName("DeleteRoleClaim");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<IResult> GetRoleClaimById(RoleClaimId id, ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetRoleClaimByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRoleClaimsByRoleId(Guid roleId, ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetRoleClaimsByRoleIdQuery(new(roleId));
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AssociateClaimToRole(AssociateClaimToRoleCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> UpdateRoleClaim(UpdateRoleClaimCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteRoleClaim(RoleClaimId id, ISender sender, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleClaimCommand(id);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }
}