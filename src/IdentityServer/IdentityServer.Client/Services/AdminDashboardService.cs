using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Dashboard;

namespace IdentityServer.Client.Services;

public class AdminDashboardService(HttpClient client) : IAdminDashboardService
{
    public Task<DashboardStatsModel?> GetStatsAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<DashboardStatsModel>("api/dashboard/stats", ClientJsonOptions.Default, ct);
}
