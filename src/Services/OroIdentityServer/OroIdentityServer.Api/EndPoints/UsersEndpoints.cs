// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroCQRS.Core.Interfaces;

namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup(string.Empty);

        api.MapGet("getuserinfo", GetUserInfo);


        return api;
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    private static async Task<Results<Ok<GetUserByIdQueryResponse>,BadRequest<string>, ProblemHttpResult>> GetUserInfo(
        GetUserByIdQuery request,
        IQueryHandler<GetUserByIdQuery,GetUserByIdQueryResponse> handler,
        CancellationToken cancellationToken
    )
    {
        return TypedResults.Ok(await handler.HandleAsync(request, cancellationToken));
    }
}