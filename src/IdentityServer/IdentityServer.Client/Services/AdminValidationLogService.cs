using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Diagnostics;

namespace IdentityServer.Client.Services;

public class AdminValidationLogService(HttpClient client) : IAdminValidationLogService
{
    public async Task<IReadOnlyList<DailyValidationCountModel>> GetDailySummaryAsync(int days = 7, CancellationToken ct = default)
    {
        var response = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<DailyValidationCountModel>>>(
            $"api/validation-logs/daily-summary?days={days}", ClientJsonOptions.Default, ct);

        return response?.Data ?? [];
    }
}
