// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class ScopeCommandsEndpoints
{
    public static RouteGroupBuilder MapScopeCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/scopes");

        api.MapPost("/create", CreateScope);
        api.MapPut("/update", UpdateScope);
        api.MapDelete("/delete/{name}", DeleteScope);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateScope(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] CreateScopeCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateScope(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] UpdateScopeCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeleteScope(
        HttpContext context,
        [FromRoute] string name,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteScopeCommand(name), cancellationToken);
        return TypedResults.Ok();
    }
}