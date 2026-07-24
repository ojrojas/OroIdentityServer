// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Queries;

public class GetValidationLogDailySummaryQueryHandler(
    ILogger<GetValidationLogDailySummaryQueryHandler> logger,
    IAuthValidationLogRepository authValidationLogRepository)
: IQueryHandler<GetValidationLogDailySummaryQuery, GetValidationLogDailySummaryResponse>
{
    public async Task<GetValidationLogDailySummaryResponse> HandleAsync(GetValidationLogDailySummaryQuery query, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetValidationLogDailySummaryQuery for Days: {Days}", query.Days);

        try
        {
            var sinceUtc = DateTime.UtcNow.AddDays(-query.Days);
            var logs = await authValidationLogRepository.GetSinceAsync(sinceUtc, cancellationToken);

            var grouped = logs
                .GroupBy(l => DateOnly.FromDateTime(l.OccurredAtUtc.Date))
                .ToDictionary(g => g.Key, g => (
                    Succeeded: g.Count(x => x.Succeeded),
                    Failed: g.Count(x => !x.Succeeded)));

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var days = new List<DailyValidationCount>();
            for (var i = query.Days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var counts = grouped.TryGetValue(date, out var value) ? value : (Succeeded: 0, Failed: 0);
                days.Add(new DailyValidationCount(date, counts.Succeeded, counts.Failed));
            }

            logger.LogInformation("Successfully handled GetValidationLogDailySummaryQuery for Days: {Days}", query.Days);
            return new GetValidationLogDailySummaryResponse(days);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetValidationLogDailySummaryQuery for Days: {Days}", query.Days);
            throw;
        }
    }
}
