using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.Dashboard;
using IdentityServer.Client.Services;
using OroIdentityServer.Application.Modules.Dashboard.DTOs;
using OroIdentityServer.Application.Modules.Dashboard.Queries;

namespace IdentityServer.Services;

public class ServerAdminDashboardService(
    IQueryDispatcher queryDispatcher,
    ICurrentTenantContext tenantContext) : IAdminDashboardService
{
    public async Task<DashboardStatsModel?> GetStatsAsync(CancellationToken ct = default)
    {
        var stats = await queryDispatcher.SendAsync(new GetDashboardStatsQuery(tenantContext.CurrentTenantId), ct);
        return Map(stats);
    }

    private static DashboardStatsModel Map(DashboardStatsDto stats) => new(
        stats.UsersCreatedToday,
        stats.RolesCreatedToday,
        stats.TenantsCreatedToday,
        stats.IdentificationTypesCreatedToday,
        stats.ConnectedUsers,
        stats.RecentlyCreated.Select(MapRecent).ToList());

    private static RecentEntityModel MapRecent(RecentEntityDto entity) => new(
        entity.Name,
        entity.TypeKey,
        entity.Href,
        entity.AvatarSeed,
        entity.CreatedAtUtc);
}
