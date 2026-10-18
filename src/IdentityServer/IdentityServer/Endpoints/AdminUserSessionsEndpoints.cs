using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.UserSessions;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUserSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/user-sessions");

        // QUERY is what the Blazor console sends; GET is what everything else can actually issue.
        // A browser SPA or a plain HttpClient has no way to send the QUERY verb, so this endpoint
        // was unreachable outside the console. The user id binds from the query string either way,
        // so both verbs can share the same handler.
        g.MapMethods("/by-user", [HttpMethods.Query, HttpMethods.Get], async (
            [FromQuery] Guid userId,
            [FromServices] IAdminUserSessionService service,
            CancellationToken ct)
            => Results.Ok(await service.GetByUserAsync(userId, ct)));

        g.MapGet("/active", async ([FromServices] IAdminUserSessionService service, CancellationToken ct)
            => Results.Ok(await service.GetActiveSessionsAsync(ct)));

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
