using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class SessionsEndpoints
{
    public static RouteGroupBuilder MapSessionsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/sessions").WithTags("Sessions");

        group.MapGet(string.Empty, GetAllSessions);
        group.MapGet("users/{userId:guid}", GetUserSessions);
        group.MapDelete("{id:guid}", TerminateSession);

        return group;
    }

    private static async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetAllSessions(
        HttpContext context,
        [FromServices] ISessionsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllSessionsAsync(cancellationToken);
        return result;
    }

    private static async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetUserSessions(
        HttpContext context,
        [FromRoute] Guid userId,
        [FromServices] ISessionsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetUserSessionsAsync(userId, cancellationToken);
        return result;
    }

    private static async Task<IResult> TerminateSession(
        HttpContext context,
        Guid id,
        [FromServices] ISessionsService service,
        CancellationToken cancellationToken)
    {
        await service.TerminateSessionAsync(id, cancellationToken);
        return TypedResults.Ok();
    }
}
