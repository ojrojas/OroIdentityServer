// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapRoles(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/roles").RequireAuthorization("AdminOnly");

        // Same reasoning as /api/applications: QUERY carries the search body the console sends, GET is
        // what a browser SPA can actually issue.
        // The QUERY verb carries the search body the Blazor console sends. A browser SPA cannot send a
        // body with GET, so the same query is reachable as GET with the filters in the query string -
        // it used to 404 there, which is why the roles list never rendered in oroidentity-admin.
        g.MapMethods("/", [HttpMethods.Query], async (
            [FromBody] PagedRequest? request,
            [FromServices] IAdminRoleService service,
            CancellationToken ct)
            => Results.Ok(await service.GetRolesAsync(request, ct)));

        g.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromServices] IAdminRoleService service,
            CancellationToken ct)
            => Results.Ok(await service.GetRolesAsync(
                new PagedRequest
                {
                    SearchTerm = search,
                    PageNumber = page ?? 1,
                    PageSize = pageSize ?? 20
                }, ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminRoleService service, CancellationToken ct)
            => Results.Ok(await service.GetRoleByIdAsync(id, ct)));

        g.MapPost("/", async ([FromBody] CreateRoleRequest request, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateRoleAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateRoleRequest request, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateRoleAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteRoleAsync(id, ct), ct));

        g.MapPost("/{id:guid}/activate", async (Guid id, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.ActivateRoleAsync(id, ct), ct));

        g.MapPut("/{id:guid}/permissions", async (Guid id, [FromBody] SetRolePermissionsRequest request, [FromServices] IAdminRoleService service, CancellationToken ct)
            => await ToResultAsync(await service.SetRolePermissionsAsync(id, request, ct), ct));
    }
}
