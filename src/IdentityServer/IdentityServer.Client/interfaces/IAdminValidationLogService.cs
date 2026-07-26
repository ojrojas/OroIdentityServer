using IdentityServer.Client.Models.Diagnostics;

namespace IdentityServer.Client.Interfaces;

public interface IAdminValidationLogService
{
    Task<IReadOnlyList<DailyValidationCountModel>> GetDailySummaryAsync(int days = 7, CancellationToken ct = default);
    Task<IReadOnlyList<ValidationLogEntryModel>> GetRecentAsync(int take = 6, CancellationToken ct = default);
}
