using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.Hierarchy;
using Microsoft.AspNetCore.Mvc;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapHierarchy(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/hierarchy");

        // POST /api/hierarchy/relationships
        g.MapPost("/relationships", async ([FromBody] CreateRelationshipRequest request, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateRelationshipAsync(request, ct), ct))
            .RequireAuthorization("CanManageHierarchy");

        // PUT /api/hierarchy/relationships/{id}/priority
        g.MapPut("/relationships/{id:guid}/priority", async (Guid id, [FromBody] UpdatePriorityRequest request, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdatePriorityAsync(id, request, ct), ct))
            .RequireAuthorization("CanManageHierarchy");

        // DELETE /api/hierarchy/relationships/{id}
        g.MapDelete("/relationships/{id:guid}", async (Guid id, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteRelationshipAsync(id, ct), ct))
            .RequireAuthorization("CanManageHierarchy");

        // GET /api/hierarchy/relationships/{userId}
        g.MapGet("/relationships/{userId:guid}", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetRelationshipsAsync(userId, ct)));

        // GET /api/hierarchy/superiors and /superiors/{userId}
        g.MapGet("/superiors", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetSuperiorsAsync(null, ct)));
        g.MapGet("/superiors/{userId:guid}", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetSuperiorsAsync(userId, ct)));

        // GET primary
        g.MapGet("/superiors/primary", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetPrimarySuperiorAsync(null, ct)));
        g.MapGet("/superiors/{userId:guid}/primary", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct) =>
        {
            var result = await service.GetPrimarySuperiorAsync(userId, ct);
            return result is null ? Results.Ok(null) : Results.Ok(result);
        });

        // GET by-type
        g.MapGet("/superiors/by-type/{type}", async (string type, [FromServices] IAdminHierarchyService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.GetSuperiorsByTypeAsync(null, type, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });
        g.MapGet("/superiors/{userId:guid}/by-type/{type}", async (Guid userId, string type, [FromServices] IAdminHierarchyService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.GetSuperiorsByTypeAsync(userId, type, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        // GET /api/hierarchy/subordinates
        g.MapGet("/subordinates", async (string? type, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetSubordinatesAsync(null, type, ct)))
            .RequireAuthorization("CanViewSubordinates");
        g.MapGet("/subordinates/{userId:guid}", async (Guid userId, string? type, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetSubordinatesAsync(userId, type, ct)))
            .RequireAuthorization("CanViewSubordinates");

        g.MapGet("/subordinates/all", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetAllSubordinatesAsync(null, ct)))
            .RequireAuthorization("CanViewSubordinates");
        g.MapGet("/subordinates/{userId:guid}/all", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetAllSubordinatesAsync(userId, ct)))
            .RequireAuthorization("CanViewSubordinates");

        // GET /api/hierarchy/chain
        g.MapGet("/chain", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetCommandChainAsync(null, ct)));
        g.MapGet("/chain/{userId:guid}", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetCommandChainAsync(userId, ct)));

        // GET /api/hierarchy/tree
        g.MapGet("/tree", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetTreeAsync(ct)));

        g.MapGet("/tree/full", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetFullTreeAsync(ct)));

        // GET /api/hierarchy/can-command/{commanderId:guid}/{targetId:guid}
        g.MapGet("/can-command/{commanderId:guid}/{targetId:guid}", async (Guid commanderId, Guid targetId, string? type, [FromServices] IAdminHierarchyService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.CanCommandAsync(commanderId, targetId, type, ct)); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        // GET /api/hierarchy/level
        g.MapGet("/level", async ([FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetLevelAsync(null, ct)));
        g.MapGet("/level/{userId:guid}", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => Results.Ok(await service.GetLevelAsync(userId, ct)));

        // POST /api/hierarchy/sync-primary/{userId}
        g.MapPost("/sync-primary/{userId:guid}", async (Guid userId, [FromServices] IAdminHierarchyService service, CancellationToken ct)
            => await ToResultAsync(await service.SyncPrimaryAsync(userId, ct), ct))
            .RequireAuthorization("CanManageHierarchy");
    }
}
