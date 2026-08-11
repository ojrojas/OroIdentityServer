using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapDashboard(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/stats", async ([FromServices] IAdminDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetStatsAsync(ct)));
    }
}
