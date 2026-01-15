// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class ApplicationQueriesEndpoints
{
    public static RouteGroupBuilder MapApplicationQueriesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/applications");

        api.MapGet("/", GetApplications)
            .WithName("GetApplications");

        api.MapGet("/{clientId}", GetApplicationByClientId)
            .WithName("GetApplicationByClientId");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok<IEnumerable<OpenIddictApplicationDescriptor>>, BadRequest<string>, ProblemHttpResult>> GetApplications(
        HttpContext context,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetApplicationsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OpenIddictApplicationDescriptor>, BadRequest<string>, ProblemHttpResult>> GetApplicationByClientId(
        HttpContext context,
        [FromRoute] string clientId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetApplicationByClientIdQuery(clientId), cancellationToken);
        return TypedResults.Ok(result);
    }
}