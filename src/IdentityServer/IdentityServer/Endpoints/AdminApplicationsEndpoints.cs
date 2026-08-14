using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.OpenIddict;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapOpenIddictApplications(this RouteGroupBuilder api)
    {
        // The OIDC application catalogue is global; only the master admin (held by the
        // is_master_admin claim) can list/create/edit/delete OIDC clients.
        var g = api.MapGroup("/applications").RequireAuthorization("MasterAdminOnly");

        g.MapGet("/", async ([FromServices] IAdminApplicationService service, CancellationToken ct)
            => Results.Ok(await service.GetApplicationsAsync(ct)));

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
