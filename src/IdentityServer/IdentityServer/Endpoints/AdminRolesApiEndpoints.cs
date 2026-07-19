// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Roles;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapRoles(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/roles");

        g.MapGet("/", async ([FromServices] IAdminRoleService service, CancellationToken ct)
            => Results.Ok(await service.GetRolesAsync(ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminRoleService service, CancellationToken ct)
            => Results.Ok(await service.GetRoleByIdAsync(id, ct)));

        g.MapPost("/", async ([FromBody] CreateRoleRequest request, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateRoleAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateRoleRequest request, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateRoleAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteRoleAsync(id, ct), ct));
    }
}
