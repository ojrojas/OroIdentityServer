// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Mvc;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Application.Modules.Users.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUsers(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/users");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetUsersQuery, GetUsersQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUsersQuery(), ct)));

        g.MapPost("/", async (
            [FromBody] CreateUserCommand cmd,
            [FromServices] ICommandHandler<CreateUserCommand> h,
            CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created($"/api/users", null);
        });

        g.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUserCommand cmd,
            [FromServices] ICommandHandler<UpdateUserCommand, UpdateUserResponse> h,
            CancellationToken ct) =>
        {
            var result = await h.HandleAsync(cmd with { UserId = id }, ct);
            return Results.Ok(result);
        });

        g.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] ICommandHandler<DeleteUserCommand> h,
            CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteUserCommand(id), ct);
            return Results.NoContent();
        });
    }
}



