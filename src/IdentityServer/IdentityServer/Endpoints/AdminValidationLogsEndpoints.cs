using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapValidationLogs(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/validation-logs");

        g.MapGet("/daily-summary", async ([FromServices] IAdminValidationLogService service, CancellationToken ct, int days = 7) =>
        {
            return Results.Ok(await service.GetDailySummaryAsync(days, ct));
        });
    }
}
