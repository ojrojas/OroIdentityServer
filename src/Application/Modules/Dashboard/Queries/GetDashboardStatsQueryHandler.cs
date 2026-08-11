// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Application.Modules.Dashboard.DTOs;
using OroIdentityServer.Infraestructure;

namespace OroIdentityServer.Application.Modules.Dashboard.Queries;

public class GetDashboardStatsQueryHandler(IDbContextFactory<OroIdentityAppContext> contextFactory)
    : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> HandleAsync(GetDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        // Use a dedicated short-lived, read-only context so this aggregation never shares the
        // circuit's scoped DbContext. Sharing it lets interleaved async operations (e.g. the nav
        // tenant switcher) trigger "a second operation was started on this context instance".
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var usersQuery = context.Users.AsNoTracking()
            .Where(u => u.CreatedAtUtc >= today && u.CreatedAtUtc < tomorrow);
        if (query.TenantId is { } tenantId)
            usersQuery = usersQuery.Where(u => u.TenantId == new TenantId(tenantId));
        var usersCreatedToday = await usersQuery.CountAsync(cancellationToken);

        var rolesCreatedToday = await context.Roles.AsNoTracking()
            .CountAsync(r => r.CreatedAtUtc >= today && r.CreatedAtUtc < tomorrow, cancellationToken);

        var tenantsCreatedToday = await context.Tenants.AsNoTracking()
            .CountAsync(t => t.CreatedAtUtc >= today && t.CreatedAtUtc < tomorrow, cancellationToken);

        var identificationTypesCreatedToday = await context.IdentificationTypes.AsNoTracking()
            .CountAsync(i => i.CreatedAtUtc >= today && i.CreatedAtUtc < tomorrow, cancellationToken);

        var activeUserIds = await context.UserSessions.AsNoTracking()
            .Where(s => s.ExpiresAt > DateTime.UtcNow)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var connectedUsers = query.TenantId is null
            ? activeUserIds.Count
            : await CountConnectedUsersForTenantAsync(context, activeUserIds, query.TenantId.Value, cancellationToken);

        var recentlyCreated = await BuildRecentlyCreatedAsync(context, query.TenantId, cancellationToken);

        return new DashboardStatsDto(
            usersCreatedToday,
            rolesCreatedToday,
            tenantsCreatedToday,
            identificationTypesCreatedToday,
            connectedUsers,
            recentlyCreated);
    }

    private static async Task<int> CountConnectedUsersForTenantAsync(
        OroIdentityAppContext context, List<UserId?> activeUserIds, Guid tenantId, CancellationToken cancellationToken)
    {
        var tenantUserIds = await context.Users.AsNoTracking()
            .Where(u => u.TenantId == new TenantId(tenantId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        return activeUserIds
            .Where(id => id is not null && tenantUserIds.Contains(id))
            .Distinct()
            .Count();
    }

    private static async Task<IReadOnlyList<RecentEntityDto>> BuildRecentlyCreatedAsync(
        OroIdentityAppContext context, Guid? tenantId, CancellationToken cancellationToken)
    {
        var entries = new List<RecentEntityDto>();

        var users = await context.Users.AsNoTracking().ToListAsync(cancellationToken);
        if (tenantId is not null)
            users = users.Where(u => u.TenantId?.Value == tenantId).ToList();

        foreach (var user in users)
        {
            var display = $"{user.Name} {user.LastName}".Trim();
            if (display.Length == 0) display = user.UserName ?? "-";
            entries.Add(new RecentEntityDto(display, "StatUsers", $"/users/{user.Id.Value}", display, user.CreatedAtUtc));
        }

        var roles = await context.Roles.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var role in roles.Where(r => r.Name is not null))
        {
            entries.Add(new RecentEntityDto(role.Name!.Value, "StatRoles", $"/roles/{role.Id.Value}", role.Name.Value, role.CreatedAtUtc));
        }

        var tenants = await context.Tenants.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var tenant in tenants)
        {
            entries.Add(new RecentEntityDto(tenant.Name.Value, "StatTenants", $"/tenants/{tenant.Id.Value}", tenant.Name.Value, tenant.CreatedAtUtc));
        }

        var identificationTypes = await context.IdentificationTypes.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var identificationType in identificationTypes)
        {
            entries.Add(new RecentEntityDto(
                identificationType.Name.Value,
                "NavIdentificationTypes",
                $"/identification-types/{identificationType.Id.Value}",
                identificationType.Name.Value,
                identificationType.CreatedAtUtc));
        }

        var today = DateTime.UtcNow.Date;
        var createdToday = entries.Where(e => e.CreatedAtUtc.Date == today).ToList();

        // Prefer today's records, fall back to the most recent ones on quiet days.
        return [.. (createdToday.Count > 0 ? createdToday : entries)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(5)];
    }
}
