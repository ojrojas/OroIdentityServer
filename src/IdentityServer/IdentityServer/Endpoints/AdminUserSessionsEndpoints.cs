using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Application.Modules.UserSessions.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUserSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/user-sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IQueryHandler<GetUserSessionsByUserQuery, IEnumerable<OroIdentityServer.Core.Modules.UserSessions.Aggregates.UserSession>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUserSessionsByUserQuery(userId), ct)));

        g.MapPost("/", async ([FromBody] CreateUserSessionCommand cmd, [FromServices] ICommandHandler<CreateUserSessionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/user-sessions", null);
        });

        g.MapPost("/{id:guid}/deactivate", async (Guid id, [FromServices] ICommandHandler<DeactivateUserSessionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeactivateUserSessionCommand(id), ct);
            return Results.NoContent();
        });
    }
}
