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

        g.MapPost("/", async ([FromBody] CreateUserSessionRequest request, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateUserSessionAsync(request, ct), ct));

        g.MapPost("/{id:guid}/deactivate", async (Guid id, [FromServices] IAdminUserSessionService service, CancellationToken ct)
            => await ToResultAsync(await service.DeactivateUserSessionAsync(id, ct), ct));
    }
}
