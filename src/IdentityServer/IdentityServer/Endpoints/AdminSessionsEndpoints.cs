using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IAdminSessionService service, CancellationToken ct)
            => Results.Ok(await service.GetByUserAsync(userId, ct)));
    }
}
