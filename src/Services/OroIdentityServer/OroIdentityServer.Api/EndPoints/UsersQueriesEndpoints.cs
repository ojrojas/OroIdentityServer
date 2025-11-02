// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class UsersQueriesEndpoints
{
    public static RouteGroupBuilder MapUsersQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup(string.Empty);

        api.MapGet("/getuserinfo/{id:guid}", GetUserInfo);

        return api;
    }

    private static async Task<Results<Ok<GetUserByIdQueryResponse>, BadRequest<string>, ProblemHttpResult>> GetUserInfo(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        return TypedResults.Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken));
    }
}