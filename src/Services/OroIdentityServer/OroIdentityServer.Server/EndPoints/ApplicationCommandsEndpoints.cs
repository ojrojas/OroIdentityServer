// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class ApplicationCommandsEndpoints
{
    public static RouteGroupBuilder MapApplicationCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("applications");

        api.MapPost("", CreateApplication)
            .WithName("CreateApplication");

        api.MapPut("/{clientId}", UpdateApplication)
            .WithName("UpdateApplication");

        api.MapDelete("/{clientId}", DeleteApplication)
            .WithName("DeleteApplication");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateApplication(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] CreateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateApplication(
        HttpContext context,
        [FromServices] ISender sender,
        [FromRoute] string clientId,
        [FromBody] UpdateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeleteApplication(
        HttpContext context,
        [FromRoute] string clientId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteApplicationCommand(clientId), cancellationToken);
        return TypedResults.Ok();
    }
}