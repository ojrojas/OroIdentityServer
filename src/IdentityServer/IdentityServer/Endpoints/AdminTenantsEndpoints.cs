using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.Tenants.Commands;
using OroIdentityServer.Application.Modules.Tenants.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapTenants(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/tenants");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetTenantsQuery, GetTenantsResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantsQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IQueryHandler<GetTenantByIdQuery, GetTenantByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantByIdQuery(id), ct)));

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IQueryHandler<GetTenantsByUserIdQuery, GetTenantsByUserIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantsByUserIdQuery(userId), ct)));

        g.MapPost("/", async ([FromBody] CreateTenantCommand cmd, [FromServices] ICommandHandler<CreateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/tenants", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTenantCommand cmd, [FromServices] ICommandHandler<UpdateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { TenantId = id }, ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/activate", async (Guid id, [FromServices] ICommandHandler<ActivateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new ActivateTenantCommand(id), ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/suspend", async (Guid id, [FromServices] ICommandHandler<SuspendTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new SuspendTenantCommand(id), ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/users", async (Guid id, [FromBody] AddTenantUserCommand cmd, [FromServices] ICommandHandler<AddTenantUserCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { TenantId = id }, ct);
            return Results.NoContent();
        });
    }
}
