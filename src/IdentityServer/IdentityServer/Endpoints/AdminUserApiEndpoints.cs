// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Users;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUsers(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/users");

        g.MapGet("/", async ([FromServices] IAdminUserService service, CancellationToken ct)
            => Results.Ok(await service.GetUsersAsync(ct)));

        g.MapPost("/", async (
            [FromBody] CreateUserRequest request,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.CreateUserAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUserRequest request,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.UpdateUserAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.DeleteUserAsync(id, ct), ct));
    }
}
