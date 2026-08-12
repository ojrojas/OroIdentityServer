using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapDashboard(this IEndpointRouteBuilder app)
    {
        // Accessible to any authenticated user: the dashboard page is [Authorize], so the
        // stats it loads must not require the ManagerOrAdmin policy (Members would get 403).
        app.MapGet("/api/dashboard/stats", async ([FromServices] IAdminDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetStatsAsync(ct)))
            .RequireAuthorization();
    }
}
