// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Http.HttpResults;
using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OpenIddict.Abstractions;
using OroBuildingBlocks.ServiceDefaults;

namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

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
        [FromServices] ISessionRepository sessionRepository,
        [FromServices] IOpenIddictAuthorizationManager authorizationManager,
        [FromServices] IOpenIddictTokenManager tokenManager,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetSessionByIdAsync(new SessionId(sessionId), cancellationToken);
        if (session == null)
        {
            return TypedResults.BadRequest("Session not found");
        }

        var subject = session.UserId.Value.ToString();

        try
        {
            // Revoke authorizations for this subject
            await authorizationManager.RevokeAsync(
                subject: subject,
                client: null,
                status: Statuses.Valid,
                type: null,
                cancellationToken: cancellationToken);

            // Revoke tokens for this subject (best-effort)
            await tokenManager.RevokeAsync(
                subject: subject,
                client: null,
                status: Statuses.Valid,
                type: null,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Log and continue with marking session ended
        }

        await sender.Send(new TerminateSessionCommand(new(sessionId)), cancellationToken);
        return TypedResults.Ok();
    }
}
