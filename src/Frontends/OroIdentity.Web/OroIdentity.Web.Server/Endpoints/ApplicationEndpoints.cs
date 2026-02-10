using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class ApplicationEndpoints
{
    public static RouteGroupBuilder MapApplicationEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/applications")
            .WithTags("Applications");

        group.MapGet(string.Empty, GetAllApplications);
        group.MapGet("/{clientid}", GetApplicationByClientId);
        group.MapPost(string.Empty, CreateApplication);
        group.MapPatch(string.Empty, UpdateApplication);

        return group;
    }

     private static async Task<IResult> UpdateApplication(
        HttpContext context,
        [FromServices] IApplicationsService service,
        [FromBody] ApplicationViewModel application,
        CancellationToken cancellationToken)
    {
        await service.UpdateApplicationAsync(application, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> CreateApplication(
        HttpContext context,
        [FromServices] IApplicationsService service,
        [FromBody] ApplicationViewModel application,
        CancellationToken cancellationToken)
    {
        await service.CreateApplicationAsync(application, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<ApplicationViewModel> GetApplicationByClientId(
        HttpContext context,
        string clientId,
        [FromServices] IApplicationsService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetApplicationByClientIdAsync(clientId, cancellationToken);
    }

    private static async Task<IEnumerable<ApplicationViewModel>> GetAllApplications(
        HttpContext context,
        [FromServices] IApplicationsService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetAllApplicationAsync(cancellationToken);
    }
}