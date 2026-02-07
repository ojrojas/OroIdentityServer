// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;

public static class RoleQueriesEndpoints
{
    public static RouteGroupBuilder MapRoleQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("roles");

        api.MapGet("/get/{id}", GetRoleById)
            .WithName("GetRoleById");

        api.MapGet("/getall", GetAllRoles)
            .WithName("GetAllRoles");
        
        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok<GetRoleByIdResponse>, NotFound>> GetRoleById(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoleByIdQuery(new(id)), cancellationToken);
        return result.Data != null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<GetRolesResponse>, ProblemHttpResult>> GetAllRoles(
        HttpContext context,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRolesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }
}