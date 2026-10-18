using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.OpenIddict;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapOpenIddictApplications(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/applications").RequireAuthorization("MasterAdminOnly");

        // The QUERY verb carries the search body the Blazor console sends. A browser SPA cannot send a
        // body with GET, so the same query is reachable as GET with the filters in the query string -
        // it used to 404 there, which is why the applications list never rendered in
        // oroidentity-admin.
        g.MapMethods("/", [HttpMethods.Query], async (
            [FromBody] PagedRequest? request,
            [FromServices] IAdminApplicationService service,
            CancellationToken ct)
            => Results.Ok(await service.GetApplicationsAsync(request, ct)));

        g.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromServices] IAdminApplicationService service,
            CancellationToken ct)
            => Results.Ok(await service.GetApplicationsAsync(
                new PagedRequest
                {
                    SearchTerm = search,
                    PageNumber = page ?? 1,
                    PageSize = pageSize ?? 20
                }, ct)));

        g.MapGet("/{clientId}", async (string clientId, [FromServices] IAdminApplicationService service, CancellationToken ct)
            => Results.Ok(await service.GetApplicationByClientIdAsync(clientId, ct)));

        g.MapPost("/", async ([FromBody] OpenIddictApplicationModel application, [FromServices] IAdminApplicationService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateApplicationAsync(application, ct), ct));

        g.MapPut("/{clientId}", async (string clientId, [FromBody] OpenIddictApplicationModel application, [FromServices] IAdminApplicationService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateApplicationAsync(clientId, application, ct), ct));

        g.MapDelete("/{clientId}", async (string clientId, [FromServices] IAdminApplicationService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteApplicationAsync(clientId, ct), ct));
    }
}
