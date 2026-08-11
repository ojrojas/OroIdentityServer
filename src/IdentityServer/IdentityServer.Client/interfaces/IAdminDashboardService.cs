using IdentityServer.Client.Models.Dashboard;

namespace IdentityServer.Client.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardStatsModel?> GetStatsAsync(CancellationToken ct = default);
}
