// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class IdentificationTypeCommandsEndpoints
{
    public static RouteGroupBuilder MapIdentificationTypeCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("/identificationtypes");

        api.MapPost("/create", CreateIdentificationType)
            .WithName("CreateIdentificationType");

        api.MapDelete("/delete/{id}", DeleteIdentificationType)
            .WithName("DeleteIdentificationType");

        api.MapPatch("/update", UpdateIdentificationType)
            .WithName("UpdateIdentificationType");

        api.RequireAuthorization([new AuthorizeAttribute
        {
            AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        }]);

        return api;
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateIdentificationType(
        HttpContext context,
        [FromBody] CreateIdentificationTypeCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeleteIdentificationType(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteIdentificationTypeCommand(new(id)), cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateIdentificationType(
        HttpContext context,
        [FromBody] UpdateIdentificationTypeCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }
}