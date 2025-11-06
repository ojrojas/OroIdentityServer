// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class IdentificationTypeQueriesEndpoints
{
      extension(IEndpointRouteBuilder routeBuilder)
    {
        public RouteGroupBuilder MapIdentificationTypeQueriesEndpointsV1()
        {
            var api = routeBuilder.MapGroup(string.Empty);

            api.MapGet("/getall", GetAllIdentificationTypes)
            .WithName("GetAllIdentificationTypes");

            return api;
        }
    }

    private static async Task<IResult> GetAllIdentificationTypes(ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetIdentificationTypesQuery();
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}