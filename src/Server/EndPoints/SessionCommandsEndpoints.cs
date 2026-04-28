// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Modules.Sessions.Commands;

namespace OroIdentityServer.Server.Endpoints;

public static class SessionCommandsEndpoints
{
    public static RouteGroupBuilder MapSessionCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("sessions");

        api.MapDelete("/{sessionId:guid}", TerminateSession)
           .WithName("TerminateSession");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> TerminateSession(
        HttpContext context,
        [FromRoute] Guid sessionId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new TerminateSessionCommand(sessionId), cancellationToken);
        return TypedResults.Ok();
    }
}
