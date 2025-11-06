// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class ScopeQueriesEndpoints
{
    public static RouteGroupBuilder MapScopeQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/scopes");

        api.MapGet("/", GetScopes)
            .WithName("GetScopes");

        return api;
    }

     private static async Task<Results<Ok<IEnumerable<OpenIddictScopeDescriptor>>, BadRequest<string>, ProblemHttpResult>> GetScopes(
        HttpContext context,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScopesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }
}