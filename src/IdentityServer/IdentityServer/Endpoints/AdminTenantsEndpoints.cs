using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Tenants;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapTenants(this RouteGroupBuilder api)
    {
        // The nav tenant switcher lists the tenants of the logged-in user, so this lookup must be
        // available to Managers too (the /api group already requires ManagerOrAdmin), not only Admins.
        api.MapGet("/tenants/mine", async ([FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetMyTenantsAsync(ct)));

        api.MapGet("/tenants/by-user/{userId:guid}", async (Guid userId, [FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantsByUserIdAsync(userId, ct)));

        // The full tenants catalogue (create, update, suspend, list all) is reserved for the
        // master admin. Tenant admins see their own subset through /tenants/mine and
        // /tenants/by-user above, which are scoped by the caller's accessible tenants.
        var g = api.MapGroup("/tenants").RequireAuthorization("MasterAdminOnly");

        g.MapGet("/", async ([FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantsAsync(ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminTenantService service, CancellationToken ct)
            => Results.Ok(await service.GetTenantByIdAsync(id, ct)));

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
