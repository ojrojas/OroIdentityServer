using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class TenantsEndpoint
{
    public static RouteGroupBuilder MapTenantsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/tenants").WithTags("Tenants");

        group.MapGet(string.Empty, GetAllTenants);
        group.MapGet("{id:guid}", GetTenantById);
        group.MapPost(string.Empty, CreateTenant);
        group.MapPut("{id:guid}", UpdateTenant);
        group.MapDelete("{id:guid}", DeleteTenant);

        return group;
    }

    private static async Task<IResult> GetAllTenants(HttpContext context, [FromServices] ITenantsService service, CancellationToken cancellationToken)
    {
        var res = await service.GetAllTenantsAsync(cancellationToken);
        return Results.Ok(res);
    }

    private static async Task<IResult> GetTenantById(HttpContext context, Guid id, [FromServices] ITenantsService service, CancellationToken cancellationToken)
    {
        var res = await service.GetTenantByIdAsync(id, cancellationToken);
        if (res == null) return Results.NotFound();
        return Results.Ok(res);
    }

    private static async Task<IResult> CreateTenant(HttpContext context, [FromServices] ITenantsService service, [FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        await service.CreateTenantAsync(request, cancellationToken);
        return Results.Created($"/api/v1/tenants/", null);
    }

    private static async Task<IResult> UpdateTenant(HttpContext context, Guid id, [FromServices] ITenantsService service, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateTenantAsync(id, request, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTenant(HttpContext context, Guid id, [FromServices] ITenantsService service, CancellationToken cancellationToken)
    {
        await service.DeleteTenantAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
