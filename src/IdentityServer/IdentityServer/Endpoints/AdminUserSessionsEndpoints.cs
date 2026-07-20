using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.UserSessions;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUserSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/user-sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => Results.Ok(await service.GetByUserAsync(userId, ct)));

        g.MapGet("/active-count", async ([FromServices] IAdminUserSessionService service, CancellationToken ct)
            => Results.Ok(await service.GetActiveSessionsCountAsync(ct)));

        g.MapPost("/", async ([FromBody] CreateUserSessionRequest request, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateUserSessionAsync(request, ct), ct));

        g.MapPost("/{id:guid}/deactivate", async (Guid id, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => await ToResultAsync(await service.DeactivateUserSessionAsync(id, ct), ct));

        g.MapPost("/terminate-all/{userId:guid}", async (Guid userId, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => await ToResultAsync(await service.TerminateAllUserSessionsAsync(userId, ct), ct));
    }
}
