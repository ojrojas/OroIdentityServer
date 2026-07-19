// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Permissions;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapPermissions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/permissions");

        g.MapGet("/", async ([FromServices] IAdminPermissionService service, CancellationToken ct)
            => Results.Ok(await service.GetPermissionsAsync(ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminPermissionService service, CancellationToken ct)
            => Results.Ok(await service.GetPermissionByIdAsync(id, ct)));

        g.MapPost("/", async ([FromBody] CreatePermissionRequest request, [FromServices] IAdminPermissionService service, CancellationToken ct)
            => await ToResultAsync(await service.CreatePermissionAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdatePermissionRequest request, [FromServices] IAdminPermissionService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdatePermissionAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] IAdminPermissionService service, CancellationToken ct)
            => await ToResultAsync(await service.DeletePermissionAsync(id, ct), ct));
    }
}
