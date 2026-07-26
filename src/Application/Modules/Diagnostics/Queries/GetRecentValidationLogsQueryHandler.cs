// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Queries;

public class GetRecentValidationLogsQueryHandler(
    ILogger<GetRecentValidationLogsQueryHandler> logger,
    IAuthValidationLogRepository authValidationLogRepository)
: IQueryHandler<GetRecentValidationLogsQuery, GetRecentValidationLogsResponse>
{
    public async Task<GetRecentValidationLogsResponse> HandleAsync(GetRecentValidationLogsQuery query, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetRecentValidationLogsQuery for Take: {Take}", query.Take);

        try
        {
            var take = query.Take <= 0 ? 6 : query.Take;
            var logs = await authValidationLogRepository.GetRecentAsync(take, cancellationToken);

            var entries = logs
                .Select(l => new ValidationLogEntry(
                    l.OccurredAtUtc,
                    l.EventType.ToString(),
                    l.Succeeded,
                    l.UserId,
                    l.ClientId,
                    l.Scopes,
                    l.FailureReason))
                .ToList();

            logger.LogInformation("Successfully handled GetRecentValidationLogsQuery for Take: {Take}", query.Take);
            return new GetRecentValidationLogsResponse(entries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetRecentValidationLogsQuery for Take: {Take}", query.Take);
            throw;
        }
    }
}
