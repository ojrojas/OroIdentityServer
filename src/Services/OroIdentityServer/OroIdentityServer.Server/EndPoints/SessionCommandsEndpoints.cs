// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Http.HttpResults;
using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OpenIddict.Abstractions;
using OroBuildingBlocks.ServiceDefaults;
using System.Reflection;

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

        try
        {
            if (!string.IsNullOrEmpty(session.AuthorizationId))
            {
                // Attempt to find and invoke OpenIddict extension methods (RevokeByAuthorizationIdAsync) via reflection.
                bool revoked = false;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in asm.GetTypes())
                    {
                        var method = type.GetMethod("RevokeByAuthorizationIdAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                        if (method != null)
                        {
                            var parameters = method.GetParameters();
                            if (parameters.Length >= 2 && parameters[0].ParameterType.IsAssignableFrom(authorizationManager.GetType()) && parameters[1].ParameterType == typeof(string))
                            {
                                var task = (Task)method.Invoke(null, new object[] { authorizationManager, session.AuthorizationId, cancellationToken });
                                await task;
                                revoked = true;
                                break;
                            }
                        }
                    }

                    if (revoked) break;
                }

                bool tokensRevoked = false;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in asm.GetTypes())
                    {
                        var method = type.GetMethod("RevokeByAuthorizationIdAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                        if (method != null)
                        {
                            var parameters = method.GetParameters();
                            if (parameters.Length >= 2 && parameters[0].ParameterType.IsAssignableFrom(tokenManager.GetType()) && parameters[1].ParameterType == typeof(string))
                            {
                                var task = (Task)method.Invoke(null, new object[] { tokenManager, session.AuthorizationId, cancellationToken });
                                await task;
                                tokensRevoked = true;
                                break;
                            }
                        }
                    }

                    if (tokensRevoked) break;
                }

                if (!revoked && !tokensRevoked)
                {
                    var subject = session.UserId.Value.ToString();
                    await authorizationManager.RevokeAsync(
                        subject: subject,
                        client: null,
                        status: Statuses.Valid,
                        type: null,
                        cancellationToken: cancellationToken);

                    await tokenManager.RevokeAsync(
                        subject: subject,
                        client: null,
                        status: Statuses.Valid,
                        type: null,
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                var subject = session.UserId.Value.ToString();
                await authorizationManager.RevokeAsync(
                    subject: subject,
                    client: null,
                    status: Statuses.Valid,
                    type: null,
                    cancellationToken: cancellationToken);

                await tokenManager.RevokeAsync(
                    subject: subject,
                    client: null,
                    status: Statuses.Valid,
                    type: null,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception)
        {
            // Log and continue with marking session ended
        }

        await sender.Send(new TerminateSessionCommand(new(sessionId)), cancellationToken);
        return TypedResults.Ok();
    }
}
