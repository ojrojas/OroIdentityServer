using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Diagnostics;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapValidationLogs(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/validation-logs");

        g.MapGet("/daily-summary", async ([FromServices] IAdminValidationLogService service, CancellationToken ct, int days = 7) =>
        {
            // Must stay wrapped: the WASM client deserializes ApiResponse<T>, so a bare
            // list here silently decodes to an empty chart.
            var summary = await service.GetDailySummaryAsync(days, ct);
            return Results.Ok(new ApiResponse<IReadOnlyList<DailyValidationCountModel>>
            {
                Data = summary,
                StatusCode = StatusCodes.Status200OK
            });
        });

        g.MapGet("/recent", async ([FromServices] IAdminValidationLogService service, CancellationToken ct, int take = 6) =>
        {
            var entries = await service.GetRecentAsync(take, ct);
            return Results.Ok(new ApiResponse<IReadOnlyList<ValidationLogEntryModel>>
            {
                Data = entries,
                StatusCode = StatusCodes.Status200OK
            });
        });
    }
}
