// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Endpoints;

using global::OroIdentityServer.Application.Modules.Sessions.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

public static class SessionQueriesEndpoints
{
    public static RouteGroupBuilder MapSessionQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("sessions");

        api.MapGet("/users/{userId:guid}", GetUserSessions)
           .WithName("GetUserSessions");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok<GetUserSessionsQueryResponse>, ProblemHttpResult>> GetUserSessions(
        HttpContext context,
        [FromRoute] Guid userId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserSessionsQuery(userId), cancellationToken);
        return TypedResults.Ok(result);
    }
}
