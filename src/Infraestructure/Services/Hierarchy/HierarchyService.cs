// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Modules.Hierarchy.DTOs;
using OroIdentityServer.Core.Modules.Hierarchy.Entities;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;
using OroIdentityServer.Infraestructure;

namespace OroIdentityServer.Infraestructure.Services.Hierarchy;

public sealed class HierarchyService : IHierarchyService
{
    private readonly OroIdentityAppContext _context;
    private readonly HierarchyOptions _options;
    private readonly ILogger<HierarchyService> _logger;
    private readonly IMemoryCache? _cache;

    public HierarchyService(
        OroIdentityAppContext context,
        IOptions<HierarchyOptions> options,
        ILogger<HierarchyService> logger,
        IMemoryCache? cache = null)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
        _cache = cache;
    }

    private static string TreeCacheKey(TenantId tenantId, bool full) => $"hierarchy:tree:{tenantId.Value}:{(full ? "full" : "primary")}";
    private void InvalidateTreeCache(TenantId tenantId)
    {
        _cache?.Remove(TreeCacheKey(tenantId, false));
        _cache?.Remove(TreeCacheKey(tenantId, true));
    }

    public async Task<UserReportingRelationship> CreateRelationshipAsync(
        TenantId tenantId,
        UserId userId,
        UserId reportsToUserId,
        RelationshipType type,
        int priority,
        UserId? performedByUserId = null,
        CancellationToken ct = default)
    {
        if (userId.Value == reportsToUserId.Value)
            throw new InvalidOperationException("cannot be own superior: User cannot report to themselves");

        // Check users exist and tenant membership? Basic validation: user not same
        var existing = await _context.UserReportingRelationships
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.ReportsToUserId == reportsToUserId && r.RelationshipType == type)
            .FirstOrDefaultAsync(ct);
        if (existing != null && existing.IsActive)
            throw new InvalidOperationException("relationship duplicate: Reporting relationship already exists for this type and tenant");
        if (existing != null && !existing.IsActive)
        {
            // Reactivate? Instead treat as duplicate error per spec? Spec says duplicate rejected
            throw new InvalidOperationException("relationship duplicate: Reporting relationship already exists for this type and tenant");
        }

        // Enforce max superiors per user
        var superiorsCount = await _context.UserReportingRelationships
            .CountAsync(r => r.TenantId == tenantId && r.UserId == userId && r.IsActive, ct);
        if (superiorsCount >= _options.MaxSuperiorsPerUser)
            throw new InvalidOperationException("maximum superiors exceeded");

        // Cycle detection: check if reportsToUserId is already a subordinate of userId (direct or indirect)
        if (await WouldCreateCycleAsync(tenantId, userId, reportsToUserId, ct))
            throw new InvalidOperationException("cycle detected: Creating this relationship would form a cycle");

        // Also validate priority >=1 via entity Validate

        var relationship = new UserReportingRelationship(tenantId, userId, reportsToUserId, type, priority, performedByUserId);
        _context.UserReportingRelationships.Add(relationship);

        // Audit
        var audit = new RelationshipAuditLog(
            relationship.Id,
            tenantId,
            userId,
            reportsToUserId,
            type,
            RelationshipAuditAction.Created,
            performedByUserId,
            $"Created relationship {type} priority {priority}");
        _context.RelationshipAuditLogs.Add(audit);

        await _context.SaveChangesAsync(ct);

        // Sync PrimaryReportsToUserId if Functional priority 1
        if (type == RelationshipType.Functional && priority == 1)
        {
            await SyncPrimaryReportsToForUserAsync(tenantId, userId, ct);
            await _context.SaveChangesAsync(ct);
        }

        InvalidateTreeCache(tenantId);
        _logger.LogInformation("Created relationship {Id} {UserId} reports to {ReportsTo} type {Type} priority {Priority}", relationship.Id.Value, userId.Value, reportsToUserId.Value, type, priority);
        return relationship;
    }

    public async Task<UserReportingRelationship> UpdateRelationshipPriorityAsync(
        UserReportingRelationshipId relationshipId,
        int newPriority,
        UserId? performedByUserId = null,
        CancellationToken ct = default)
    {
        var relationship = await _context.UserReportingRelationships
            .IgnoreQueryFilters()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == relationshipId, ct)
            ?? throw new KeyNotFoundException($"Relationship {relationshipId.Value} not found");

        if (!relationship.IsActive)
            throw new InvalidOperationException("Cannot update inactive relationship");

        var oldPriority = relationship.Priority;
        if (oldPriority == newPriority) return relationship;

        relationship.UpdatePriority(newPriority);

        var audit = new RelationshipAuditLog(
            relationship.Id,
            relationship.TenantId,
            relationship.UserId,
            relationship.ReportsToUserId,
            relationship.RelationshipType,
            RelationshipAuditAction.PriorityChanged,
            performedByUserId,
            $"Priority changed from {oldPriority} to {newPriority}");
        _context.RelationshipAuditLogs.Add(audit);

        await _context.SaveChangesAsync(ct);

        // Sync primary if Functional
        if (relationship.RelationshipType == RelationshipType.Functional && (oldPriority == 1 || newPriority == 1))
        {
            await SyncPrimaryReportsToForUserAsync(relationship.TenantId, relationship.UserId, ct);
            await _context.SaveChangesAsync(ct);
        }

        InvalidateTreeCache(relationship.TenantId);
        _logger.LogInformation("Updated relationship {Id} priority {Old} -> {New}", relationshipId.Value, oldPriority, newPriority);
        return relationship;
    }

    public async Task DeleteRelationshipAsync(
        UserReportingRelationshipId relationshipId,
        UserId? performedByUserId = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var relationship = await _context.UserReportingRelationships
            .IgnoreQueryFilters()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == relationshipId, ct)
            ?? throw new KeyNotFoundException($"Relationship {relationshipId.Value} not found");

        if (!relationship.IsActive)
            return;

        relationship.Deactivate();

        var audit = new RelationshipAuditLog(
            relationship.Id,
            relationship.TenantId,
            relationship.UserId,
            relationship.ReportsToUserId,
            relationship.RelationshipType,
            RelationshipAuditAction.Deleted,
            performedByUserId,
            "Deleted relationship",
            reason);
        _context.RelationshipAuditLogs.Add(audit);

        await _context.SaveChangesAsync(ct);

        if (relationship.RelationshipType == RelationshipType.Functional)
        {
            await SyncPrimaryReportsToForUserAsync(relationship.TenantId, relationship.UserId, ct);
            await _context.SaveChangesAsync(ct);
        }

        InvalidateTreeCache(relationship.TenantId);
        _logger.LogInformation("Deleted relationship {Id}", relationshipId.Value);
    }

    public async Task<IReadOnlyList<UserReportingRelationship>> GetUserRelationshipsAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        return await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsActive)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.RelationshipType)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HierarchyRelationshipDto>> GetUserRelationshipsDtoAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        var list = await GetUserRelationshipsAsync(tenantId, userId, ct);
        return list.Select(r => new HierarchyRelationshipDto(r.Id.Value, r.TenantId.Value, r.UserId.Value, r.ReportsToUserId.Value, r.RelationshipType, r.Priority, r.IsActive, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList();
    }

    public async Task<IReadOnlyList<SuperiorDto>> GetSuperiorsByTypeAsync(
        TenantId tenantId,
        UserId userId,
        RelationshipType type,
        CancellationToken ct = default)
    {
        var query = from rel in _context.UserReportingRelationships
                    where rel.TenantId == tenantId && rel.UserId == userId && rel.IsActive && rel.RelationshipType == type
                    join user in _context.Users on rel.ReportsToUserId equals user.Id
                    join tenantUser in _context.TenantUsers on new { TenantId = rel.TenantId, UserId = rel.ReportsToUserId } equals new { TenantId = tenantUser.TenantId, UserId = tenantUser.UserId } into tj
                    from tu in tj.DefaultIfEmpty()
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId into urj
                    from ur in urj.DefaultIfEmpty()
                    join role in _context.Roles on ur.RoleId equals role.Id into rj
                    from role in rj.DefaultIfEmpty()
                    select new SuperiorDto(
                        rel.ReportsToUserId.Value,
                        user.UserName,
                        user.Email,
                        rel.RelationshipType,
                        rel.Priority,
                        tu != null ? tu.HierarchyLevel : (role != null ? role.Level : 10),
                        role != null ? role.Name.Value : null);

        return await query.Distinct().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuperiorDto>> GetDirectSuperiorsAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        var query = from rel in _context.UserReportingRelationships
                    where rel.TenantId == tenantId && rel.UserId == userId && rel.IsActive
                    join user in _context.Users on rel.ReportsToUserId equals user.Id
                    join tenantUser in _context.TenantUsers on new { TenantId = rel.TenantId, UserId = rel.ReportsToUserId } equals new { TenantId = tenantUser.TenantId, UserId = tenantUser.UserId } into tj
                    from tu in tj.DefaultIfEmpty()
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId into urj
                    from ur in urj.DefaultIfEmpty()
                    join role in _context.Roles on ur.RoleId equals role.Id into rj
                    from role in rj.DefaultIfEmpty()
                    select new SuperiorDto(
                        rel.ReportsToUserId.Value,
                        user.UserName,
                        user.Email,
                        rel.RelationshipType,
                        rel.Priority,
                        tu != null ? tu.HierarchyLevel : (role != null ? role.Level : 10),
                        role != null ? role.Name.Value : null);

        return await query.Distinct().ToListAsync(ct);
    }

    public async Task<SuperiorDto?> GetPrimarySuperiorAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        var rel = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .FirstOrDefaultAsync(ct);

        if (rel == null) return null;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == rel.ReportsToUserId, ct);
        if (user == null) return new SuperiorDto(rel.ReportsToUserId.Value, null, null, rel.RelationshipType, rel.Priority, 10, null);

        var tenantUser = await _context.TenantUsers.FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == rel.ReportsToUserId, ct);
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == rel.ReportsToUserId, ct);
        string? roleName = null;
        int level = 10;
        if (userRole != null)
        {
            var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == userRole.RoleId, ct);
            if (role != null)
            {
                roleName = role.Name.Value;
                level = role.Level;
            }
        }
        if (tenantUser != null) level = tenantUser.HierarchyLevel;

        return new SuperiorDto(rel.ReportsToUserId.Value, user.UserName, user.Email, rel.RelationshipType, rel.Priority, level, roleName);
    }

    public async Task<IReadOnlyList<SubordinateDto>> GetDirectSubordinatesAsync(
        TenantId tenantId,
        UserId userId,
        RelationshipType? filterType = null,
        CancellationToken ct = default)
    {
        var baseQuery = _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.ReportsToUserId == userId && r.IsActive);

        if (filterType != null)
            baseQuery = baseQuery.Where(r => r.RelationshipType == filterType.Value);

        var query = from rel in baseQuery
                    join user in _context.Users on rel.UserId equals user.Id
                    join tenantUser in _context.TenantUsers on new { TenantId = rel.TenantId, UserId = rel.UserId } equals new { TenantId = tenantUser.TenantId, UserId = tenantUser.UserId } into tj
                    from tu in tj.DefaultIfEmpty()
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId into urj
                    from ur in urj.DefaultIfEmpty()
                    join role in _context.Roles on ur.RoleId equals role.Id into rj
                    from role in rj.DefaultIfEmpty()
                    select new SubordinateDto(
                        rel.UserId.Value,
                        user.UserName,
                        user.Email,
                        rel.RelationshipType,
                        rel.Priority,
                        tu != null ? tu.HierarchyLevel : (role != null ? role.Level : 10),
                        role != null ? role.Name.Value : null);

        return await query.Distinct().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SubordinateDto>> GetAllSubordinatesAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        // Try PostgreSQL recursive CTE for performance
        if (_context.Database.ProviderName != null && _context.Database.ProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return await GetAllSubordinatesViaCteAsync(tenantId, userId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CTE query failed, falling back to in-memory traversal for GetAllSubordinates");
            }
        }
        return await GetAllSubordinatesInMemoryAsync(tenantId, userId, ct);
    }

    private async Task<IReadOnlyList<SubordinateDto>> GetAllSubordinatesViaCteAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct)
    {
        // Recursive CTE using only Functional primary relationships (Priority=1) with visited array to prevent loops
        var sql = @"
WITH RECURSIVE subordinates(""UserId"", depth, path) AS (
    SELECT urr.""UserId"", 1, ARRAY[urr.""UserId""]
    FROM ""UserReportingRelationships"" urr
    WHERE urr.""TenantId"" = {0} AND urr.""ReportsToUserId"" = {1} AND urr.""IsActive"" = true AND urr.""RelationshipType"" = 'Functional' AND urr.""Priority"" = 1
    UNION
    SELECT urr.""UserId"", s.depth + 1, s.path || urr.""UserId""
    FROM ""UserReportingRelationships"" urr
    JOIN subordinates s ON urr.""ReportsToUserId"" = s.""UserId""
    WHERE urr.""TenantId"" = {0} AND urr.""IsActive"" = true AND urr.""RelationshipType"" = 'Functional' AND urr.""Priority"" = 1
      AND NOT (urr.""UserId"" = ANY(s.path))
      AND s.depth < {2}
)
SELECT DISTINCT ""UserId"" FROM subordinates;
";
        var tenantIdParam = tenantId.Value;
        var userIdParam = userId.Value;
        var maxDepth = _options.MaxDepth;

        var ids = await _context.Database.SqlQueryRaw<Guid>(sql, tenantIdParam, userIdParam, maxDepth).ToListAsync(ct);

        if (ids.Count == 0) return Array.Empty<SubordinateDto>();

        var relations = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && ids.Contains(r.UserId.Value) && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .ToListAsync(ct);

        var result = new List<SubordinateDto>();
        foreach (var rel in relations.DistinctBy(r => r.UserId.Value))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == rel.UserId, ct);
            if (user == null) continue;
            var tu = await _context.TenantUsers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == rel.UserId, ct);
            var ur = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == rel.UserId, ct);
            string? roleName = null;
            int level = tu?.HierarchyLevel ?? 10;
            if (ur != null)
            {
                var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ur.RoleId, ct);
                if (role != null)
                {
                    roleName = role.Name.Value;
                    if (tu == null) level = role.Level;
                }
            }
            result.Add(new SubordinateDto(rel.UserId.Value, user.UserName, user.Email, rel.RelationshipType, rel.Priority, level, roleName));
        }
        return result;
    }

    private async Task<IReadOnlyList<SubordinateDto>> GetAllSubordinatesInMemoryAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<(Guid id, int depth)>();
        queue.Enqueue((userId.Value, 0));
        visited.Add(userId.Value);
        var resultIds = new List<Guid>();

        // BFS over primary relationships
        var allRelations = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .ToListAsync(ct);

        var byReportsTo = allRelations.GroupBy(r => r.ReportsToUserId.Value).ToDictionary(g => g.Key, g => g.Select(x => x.UserId.Value).ToList());

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth >= _options.MaxDepth) continue;
            if (!byReportsTo.TryGetValue(current, out var subs)) continue;
            foreach (var subId in subs)
            {
                if (visited.Contains(subId)) continue;
                visited.Add(subId);
                resultIds.Add(subId);
                queue.Enqueue((subId, depth + 1));
            }
        }

        if (resultIds.Count == 0) return Array.Empty<SubordinateDto>();

        var result = new List<SubordinateDto>();
        foreach (var id in resultIds)
        {
            var rel = allRelations.FirstOrDefault(r => r.UserId.Value == id);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == new UserId(id), ct);
            if (user == null) continue;
            var tu = await _context.TenantUsers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == new UserId(id), ct);
            var ur = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == new UserId(id), ct);
            string? roleName = null;
            int level = tu?.HierarchyLevel ?? 10;
            if (ur != null)
            {
                var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ur.RoleId, ct);
                if (role != null)
                {
                    roleName = role.Name.Value;
                    if (tu == null) level = role.Level;
                }
            }
            var relForDto = rel ?? new UserReportingRelationship(tenantId, new UserId(id), userId, RelationshipType.Functional, 1);
            result.Add(new SubordinateDto(id, user.UserName, user.Email, RelationshipType.Functional, 1, level, roleName));
        }
        return result;
    }

    public async Task<IReadOnlyList<SuperiorDto>> GetCommandChainAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        if (_context.Database.ProviderName != null && _context.Database.ProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return await GetCommandChainViaCteAsync(tenantId, userId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CTE chain query failed, falling back to in-memory");
            }
        }
        return await GetCommandChainInMemoryAsync(tenantId, userId, ct);
    }

    private async Task<IReadOnlyList<SuperiorDto>> GetCommandChainViaCteAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct)
    {
        var sql = @"
WITH RECURSIVE chain(""ReportsToUserId"", depth, path) AS (
    SELECT urr.""ReportsToUserId"", 1, ARRAY[urr.""UserId""]
    FROM ""UserReportingRelationships"" urr
    WHERE urr.""TenantId"" = {0} AND urr.""UserId"" = {1} AND urr.""IsActive"" = true AND urr.""RelationshipType"" = 'Functional' AND urr.""Priority"" = 1
    UNION
    SELECT urr.""ReportsToUserId"", c.depth + 1, c.path || urr.""UserId""
    FROM ""UserReportingRelationships"" urr
    JOIN chain c ON urr.""UserId"" = c.""ReportsToUserId""
    WHERE urr.""TenantId"" = {0} AND urr.""IsActive"" = true AND urr.""RelationshipType"" = 'Functional' AND urr.""Priority"" = 1
      AND NOT (urr.""ReportsToUserId"" = ANY(c.path))
      AND c.depth < {2}
)
SELECT DISTINCT ""ReportsToUserId"" FROM chain;
";
        var tenantIdParam = tenantId.Value;
        var userIdParam = userId.Value;
        var maxDepth = _options.MaxDepth;

        var ids = await _context.Database.SqlQueryRaw<Guid>(sql, tenantIdParam, userIdParam, maxDepth).ToListAsync(ct);
        if (ids.Count == 0) return Array.Empty<SuperiorDto>();

        var result = new List<SuperiorDto>();
        foreach (var id in ids)
        {
            var rel = await _context.UserReportingRelationships.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.UserId == userId && r.ReportsToUserId == new UserId(id) && r.IsActive, ct)
                      ?? await _context.UserReportingRelationships.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportsToUserId == new UserId(id), ct);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == new UserId(id), ct);
            if (user == null) continue;
            var tu = await _context.TenantUsers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == new UserId(id), ct);
            var ur = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == new UserId(id), ct);
            string? roleName = null;
            int level = tu?.HierarchyLevel ?? 10;
            if (ur != null)
            {
                var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ur.RoleId, ct);
                if (role != null)
                {
                    roleName = role.Name.Value;
                    if (tu == null) level = role.Level;
                }
            }
            result.Add(new SuperiorDto(id, user.UserName, user.Email, RelationshipType.Functional, 1, level, roleName));
        }
        return result;
    }

    private async Task<IReadOnlyList<SuperiorDto>> GetCommandChainInMemoryAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct)
    {
        var chain = new List<SuperiorDto>();
        var visited = new HashSet<Guid> { userId.Value };
        var currentId = userId.Value;
        var depth = 0;

        var allRelations = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .ToListAsync(ct);

        var byUser = allRelations.GroupBy(r => r.UserId.Value).ToDictionary(g => g.Key, g => g.First());

        while (depth < _options.MaxDepth)
        {
            if (!byUser.TryGetValue(currentId, out var rel)) break;
            var nextId = rel.ReportsToUserId.Value;
            if (visited.Contains(nextId)) break; // cycle
            visited.Add(nextId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == new UserId(nextId), ct);
            if (user == null) break;
            var tu = await _context.TenantUsers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == new UserId(nextId), ct);
            var ur = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == new UserId(nextId), ct);
            string? roleName = null;
            int level = tu?.HierarchyLevel ?? 10;
            if (ur != null)
            {
                var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ur.RoleId, ct);
                if (role != null)
                {
                    roleName = role.Name.Value;
                    if (tu == null) level = role.Level;
                }
            }
            chain.Add(new SuperiorDto(nextId, user.UserName, user.Email, rel.RelationshipType, rel.Priority, level, roleName));
            currentId = nextId;
            depth++;
        }
        return chain;
    }

    public async Task<bool> CanCommandAsync(
        TenantId tenantId,
        UserId commanderId,
        UserId targetId,
        CancellationToken ct = default)
    {
        if (commanderId.Value == targetId.Value) return false;
        // Commander can command if target is in its subordinate tree
        var subordinates = await GetAllSubordinatesAsync(tenantId, commanderId, ct);
        if (subordinates.Any(s => s.UserId == targetId.Value)) return true;

        // Also check direct relationships of any type where commander is superior
        var direct = await _context.UserReportingRelationships
            .AnyAsync(r => r.TenantId == tenantId && r.UserId == targetId && r.ReportsToUserId == commanderId && r.IsActive, ct);
        return direct;
    }

    public async Task<bool> CanCommandByTypeAsync(
        TenantId tenantId,
        UserId commanderId,
        UserId targetId,
        RelationshipType type,
        CancellationToken ct = default)
    {
        if (commanderId.Value == targetId.Value) return false;
        // Check direct
        var direct = await _context.UserReportingRelationships
            .AnyAsync(r => r.TenantId == tenantId && r.UserId == targetId && r.ReportsToUserId == commanderId && r.IsActive && r.RelationshipType == type, ct);
        if (direct) return true;

        // For recursive, only consider that type and primary path? We'll do BFS over that type
        var allRelations = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive && r.RelationshipType == type)
            .ToListAsync(ct);

        var visited = new HashSet<Guid> { commanderId.Value };
        var queue = new Queue<Guid>();
        queue.Enqueue(commanderId.Value);

        var byReportsTo = allRelations.GroupBy(r => r.ReportsToUserId.Value).ToDictionary(g => g.Key, g => g.Select(x => x.UserId.Value).ToList());

        int depth = 0;
        while (queue.Count > 0 && depth < _options.MaxDepth)
        {
            var levelSize = queue.Count;
            for (int i = 0; i < levelSize; i++)
            {
                var cur = queue.Dequeue();
                if (!byReportsTo.TryGetValue(cur, out var subs)) continue;
                foreach (var sub in subs)
                {
                    if (sub == targetId.Value) return true;
                    if (visited.Add(sub))
                        queue.Enqueue(sub);
                }
            }
            depth++;
        }
        return false;
    }

    public async Task<OrganizationTreeNodeDto?> GetOrganizationTreeAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var cacheKey = TreeCacheKey(tenantId, false);
        if (_cache != null && _cache.TryGetValue(cacheKey, out OrganizationTreeNodeDto? cached)) return cached;

        var relationships = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .ToListAsync(ct);

        var tree = await BuildTreeAsync(tenantId, relationships, includeSecondary: false, ct);
        if (_cache != null && tree != null) _cache.Set(cacheKey, tree, TimeSpan.FromMinutes(5));
        return tree;
    }

    public async Task<OrganizationTreeNodeDto?> GetFullOrganizationTreeAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var cacheKey = TreeCacheKey(tenantId, true);
        if (_cache != null && _cache.TryGetValue(cacheKey, out OrganizationTreeNodeDto? cached)) return cached;

        var relationships = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync(ct);

        var tree = await BuildTreeAsync(tenantId, relationships, includeSecondary: true, ct);
        if (_cache != null && tree != null) _cache.Set(cacheKey, tree, TimeSpan.FromMinutes(5));
        return tree;
    }

    private async Task<OrganizationTreeNodeDto?> BuildTreeAsync(
        TenantId tenantId,
        List<UserReportingRelationship> relationships,
        bool includeSecondary,
        CancellationToken ct)
    {
        if (relationships.Count == 0)
        {
            // Return null or empty? If no relationships, try to find root users (those not subordinate)
            return null;
        }

        // Find users that are not subordinates (roots)
        var subordinateIds = relationships.Select(r => r.UserId.Value).ToHashSet();
        var superiorIds = relationships.Select(r => r.ReportsToUserId.Value).ToHashSet();
        var rootIds = superiorIds.Except(subordinateIds).ToList();

        // If no roots found (cycle or all are both), pick first superior
        if (rootIds.Count == 0 && relationships.Count > 0)
        {
            rootIds = new List<Guid> { relationships.First().ReportsToUserId.Value };
        }

        // Build lookup for children (primary only or all?)
        // For primary tree, children are Functional priority 1
        // For full tree, primary children are Functional priority 1, secondary are others
        var primaryRelationships = relationships.Where(r => r.RelationshipType == RelationshipType.Functional && r.Priority == 1).ToList();
        var byParent = primaryRelationships.GroupBy(r => r.ReportsToUserId.Value).ToDictionary(g => g.Key, g => g.ToList());

        // For secondary, group by UserId
        var secondaryByUser = relationships
            .Where(r => !(r.RelationshipType == RelationshipType.Functional && r.Priority == 1))
            .GroupBy(r => r.UserId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // To handle async, we'll build recursively with async method
        async Task<OrganizationTreeNodeDto> BuildNodeAsync(Guid userId, HashSet<Guid> visited)
        {
            if (visited.Contains(userId))
                throw new InvalidOperationException("Cycle detected in organization tree");

            visited.Add(userId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == new UserId(userId), ct);
            var tu = await _context.TenantUsers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == new UserId(userId), ct);
            var ur = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == new UserId(userId), ct);
            string? roleName = null;
            int level = tu?.HierarchyLevel ?? 10;
            if (ur != null)
            {
                var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ur.RoleId, ct);
                if (role != null)
                {
                    roleName = role.Name.Value;
                    if (tu == null) level = role.Level;
                }
            }

            var children = new List<OrganizationTreeNodeDto>();
            if (byParent.TryGetValue(userId, out var childRels))
            {
                foreach (var rel in childRels)
                {
                    if (visited.Contains(rel.UserId.Value)) continue;
                    var childVisited = new HashSet<Guid>(visited);
                    var childNode = await BuildNodeAsync(rel.UserId.Value, childVisited);
                    children.Add(childNode);
                }
            }

            List<HierarchyRelationshipDto> secondary = new();
            if (includeSecondary && secondaryByUser.TryGetValue(userId, out var secRels))
            {
                secondary = secRels.Select(r => new HierarchyRelationshipDto(r.Id.Value, r.TenantId.Value, r.UserId.Value, r.ReportsToUserId.Value, r.RelationshipType, r.Priority, r.IsActive, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList();
            }

            return new OrganizationTreeNodeDto(
                userId,
                user?.UserName,
                user?.Email,
                roleName,
                level,
                children,
                secondary);
        }

        if (rootIds.Count == 1)
        {
            return await BuildNodeAsync(rootIds[0], new HashSet<Guid>());
        }
        else
        {
            // Multiple roots: create virtual root
            var virtualChildren = new List<OrganizationTreeNodeDto>();
            foreach (var rootId in rootIds)
            {
                var node = await BuildNodeAsync(rootId, new HashSet<Guid>());
                virtualChildren.Add(node);
            }
            // Return virtual root with null? or first root with children as siblings?
            // Spec expects single tree; we return virtual root with tenantId as id? Use empty guid for virtual
            // Instead return node with multiple children aggregated under first? Simpler: create synthetic root
            return new OrganizationTreeNodeDto(
                Guid.Empty,
                "Organization",
                null,
                null,
                0,
                virtualChildren,
                new List<HierarchyRelationshipDto>());
        }
    }

    public async Task<int> GetHierarchyLevelAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        var tenantUser = await _context.TenantUsers.FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId, ct);
        if (tenantUser != null) return tenantUser.HierarchyLevel;

        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId, ct);
        if (userRole != null)
        {
            var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == userRole.RoleId, ct);
            if (role != null) return role.Level;
        }
        return 10;
    }

    public async Task SyncPrimaryReportsToAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default)
    {
        await SyncPrimaryReportsToForUserAsync(tenantId, userId, ct);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SyncPrimaryReportsToForUserAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct)
    {
        var primary = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsActive && r.RelationshipType == RelationshipType.Functional && r.Priority == 1)
            .OrderBy(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        var tenantUser = await _context.TenantUsers.AsTracking().FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId, ct);
        if (tenantUser == null) return;

        var newPrimaryId = primary?.ReportsToUserId;
        tenantUser.SetPrimaryReportsTo(newPrimaryId);

        // Also sync hierarchy level
        var level = await GetHierarchyLevelAsync(tenantId, userId, ct);
        // If tenantUser level differs from role level, update? Already stored
        // We keep as is, but ensure if userRole changed, we sync
        // For now, ensure tenantUser hierarchy level matches role
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId, ct);
        if (userRole != null)
        {
            var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == userRole.RoleId, ct);
            if (role != null && tenantUser.HierarchyLevel != role.Level)
            {
                tenantUser.SetHierarchyLevel(role.Level);
            }
        }
    }

    private async Task<bool> WouldCreateCycleAsync(
        TenantId tenantId,
        UserId userId,
        UserId reportsToUserId,
        CancellationToken ct)
    {
        // If reportsToUserId is descendant of userId via any active relationship, cycle
        var allActive = await _context.UserReportingRelationships
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .Select(r => new { r.UserId, r.ReportsToUserId })
            .ToListAsync(ct);

        var graph = allActive.GroupBy(x => x.ReportsToUserId.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserId.Value).ToList());

        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(userId.Value);
        visited.Add(userId.Value);

        int depth = 0;
        while (queue.Count > 0 && depth < _options.MaxDepth)
        {
            var levelSize = queue.Count;
            for (int i = 0; i < levelSize; i++)
            {
                var cur = queue.Dequeue();
                if (!graph.TryGetValue(cur, out var subs)) continue;
                foreach (var sub in subs)
                {
                    if (sub == reportsToUserId.Value) return true;
                    if (visited.Add(sub))
                        queue.Enqueue(sub);
                }
            }
            depth++;
        }
        return false;
    }
}
