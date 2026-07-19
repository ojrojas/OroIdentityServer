using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Tenants;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapTenants(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/tenants");

        g.MapGet("/", async ([FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantsAsync(ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantByIdAsync(id, ct)));

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantsByUserIdAsync(userId, ct)));

        g.MapPost("/", async ([FromBody] CreateTenantRequest request, [FromServices] IAdminTenantService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateTenantAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTenantRequest request, [FromServices] IAdminTenantService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateTenantAsync(id, request, ct), ct));

        g.MapPost("/{id:guid}/activate", async (Guid id, [FromServices] IAdminTenantService service, CancellationToken ct)
            => await ToResultAsync(await service.ActivateTenantAsync(id, ct), ct));

        g.MapPost("/{id:guid}/suspend", async (Guid id, [FromServices] IAdminTenantService service, CancellationToken ct)
            => await ToResultAsync(await service.SuspendTenantAsync(id, ct), ct));

        g.MapPost("/{id:guid}/users", async (Guid id, [FromBody] AddTenantUserRequest request, [FromServices] IAdminTenantService service, CancellationToken ct)
            => await ToResultAsync(await service.AddTenantUserAsync(id, request, ct), ct));
    }
}
