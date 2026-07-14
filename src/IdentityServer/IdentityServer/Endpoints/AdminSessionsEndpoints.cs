using Microsoft.AspNetCore.Mvc;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Application.Modules.Sessions.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, [FromServices] IQueryHandler<GetUserSessionsQuery, GetUserSessionsQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUserSessionsQuery(userId), ct)));
    }
}
