// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.Permissions.Commands;
using OroIdentityServer.Application.Modules.Permissions.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapPermissions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/permissions");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetPermissionsQuery, GetPermissionsQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetPermissionsQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IQueryHandler<GetPermissionByIdQuery, GetPermissionByIdQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetPermissionByIdQuery(id), ct)));

        g.MapPost("/", async ([FromBody] CreatePermissionCommand cmd, [FromServices] ICommandHandler<CreatePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/permissions", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdatePermissionCommand cmd, [FromServices] ICommandHandler<UpdatePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { PermissionId = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] ICommandHandler<DeletePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeletePermissionCommand(id), ct);
            return Results.NoContent();
        });
    }
}