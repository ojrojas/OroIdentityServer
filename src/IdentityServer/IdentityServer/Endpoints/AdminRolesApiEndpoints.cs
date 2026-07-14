// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Application.Modules.Roles.Commands;
using OroIdentityServer.Application.Modules.Roles.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapRoles(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/roles");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetRolesQuery, GetRolesResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetRolesQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IQueryHandler<GetRoleByIdQuery, GetRoleByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetRoleByIdQuery(id), ct)));

        g.MapPost("/", async ([FromBody] CreateRoleCommand cmd, [FromServices] ICommandHandler<CreateRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/roles", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateRoleCommand cmd, [FromServices] ICommandHandler<UpdateRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Id = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] ICommandHandler<DeleteRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteRoleCommand(id), ct);
            return Results.NoContent();
        });
    }
}
