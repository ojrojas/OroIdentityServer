// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Infraestructure.Services.Hierarchy;
using OroIdentityServer.Core.Modules.Tenants.Entities;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Shared;

namespace Infraestructure.UnitTests;

public class HierarchyServiceTests
{
    private static OroIdentityAppContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OroIdentityAppContext(options);
    }

    private static HierarchyService CreateService(OroIdentityAppContext ctx)
    {
        var opts = Options.Create(new HierarchyOptions { MaxDepth = 10, MaxSuperiorsPerUser = 5, MaxSubordinatesPerUser = 1000 });
        return new HierarchyService(ctx, opts, NullLogger<HierarchyService>.Instance);
    }

    [Fact]
    public async Task CreateRelationship_Functional_Primary_Syncs_TenantUser()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var superiorId = new UserId(Guid.NewGuid());

        // Seed TenantUser
        var tenantUser = new TenantUser(tenantId, userId);
        ctx.TenantUsers.Add(tenantUser);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        var rel = await service.CreateRelationshipAsync(tenantId, userId, superiorId, RelationshipType.Functional, 1);

        Assert.NotNull(rel);
        Assert.Equal(1, rel.Priority);
        Assert.Equal(RelationshipType.Functional, rel.RelationshipType);

        // Verify audit log created
        var audit = await ctx.RelationshipAuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(audit);
        Assert.Equal(OroIdentityServer.Core.Modules.Hierarchy.Entities.RelationshipAuditAction.Created, audit.Action);

        // Verify sync
        var updatedTu = await ctx.TenantUsers.FirstAsync(tu => tu.UserId == userId);
        Assert.Equal(superiorId.Value, updatedTu.PrimaryReportsToUserId!.Value);
    }

    [Fact]
    public async Task CreateRelationship_PreventDuplicate()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var superiorId = new UserId(Guid.NewGuid());
        ctx.TenantUsers.Add(new TenantUser(tenantId, userId));
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        await service.CreateRelationshipAsync(tenantId, userId, superiorId, RelationshipType.Functional, 1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRelationshipAsync(tenantId, userId, superiorId, RelationshipType.Functional, 1));
    }

    [Fact]
    public async Task CreateRelationship_PreventSelfCycle()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        ctx.TenantUsers.Add(new TenantUser(tenantId, userId));
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRelationshipAsync(tenantId, userId, userId, RelationshipType.Functional, 1));
    }

    [Fact]
    public async Task CreateRelationship_DetectIndirectCycle()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var a = new UserId(Guid.NewGuid());
        var b = new UserId(Guid.NewGuid());
        var c = new UserId(Guid.NewGuid());
        ctx.TenantUsers.AddRange(new[] { new TenantUser(tenantId, a), new TenantUser(tenantId, b), new TenantUser(tenantId, c) });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        await service.CreateRelationshipAsync(tenantId, b, a, RelationshipType.Functional, 1);
        await service.CreateRelationshipAsync(tenantId, c, b, RelationshipType.Functional, 1);
        // Now try to create a reports to c -> would form cycle a -> b -> c -> a
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRelationshipAsync(tenantId, a, c, RelationshipType.Functional, 1));
    }

    [Fact]
    public async Task UpdatePriority_SyncsPrimary()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var sup1 = new UserId(Guid.NewGuid());
        var sup2 = new UserId(Guid.NewGuid());
        ctx.TenantUsers.Add(new TenantUser(tenantId, userId));
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        var rel = await service.CreateRelationshipAsync(tenantId, userId, sup1, RelationshipType.Functional, 2);
        // Initially no primary
        var tu1 = await ctx.TenantUsers.FirstAsync(tu => tu.UserId == userId);
        Assert.Null(tu1.PrimaryReportsToUserId);
        // Update to priority 1 should sync
        await service.UpdateRelationshipPriorityAsync(rel.Id, 1);
        var tu2 = await ctx.TenantUsers.FirstAsync(tu => tu.UserId == userId);
        Assert.Equal(sup1.Value, tu2.PrimaryReportsToUserId!.Value);
    }

    [Fact]
    public async Task Delete_SoftDeletes_And_Audits()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var sup = new UserId(Guid.NewGuid());
        ctx.TenantUsers.Add(new TenantUser(tenantId, userId));
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        var rel = await service.CreateRelationshipAsync(tenantId, userId, sup, RelationshipType.Project, 1);
        await service.DeleteRelationshipAsync(rel.Id);
        var fetched = await ctx.UserReportingRelationships.IgnoreQueryFilters().FirstAsync(r => r.Id == rel.Id);
        Assert.False(fetched.IsActive);
        var auditDeleted = await ctx.RelationshipAuditLogs.CountAsync(a => a.Action == OroIdentityServer.Core.Modules.Hierarchy.Entities.RelationshipAuditAction.Deleted);
        Assert.Equal(1, auditDeleted);
    }

    [Fact]
    public async Task GetAllSubordinates_Recursive()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var root = new UserId(Guid.NewGuid());
        var child = new UserId(Guid.NewGuid());
        var grandchild = new UserId(Guid.NewGuid());
        ctx.TenantUsers.AddRange(new[] { new TenantUser(tenantId, root), new TenantUser(tenantId, child), new TenantUser(tenantId, grandchild) });
        SeedUser(ctx, tenantId, root);
        SeedUser(ctx, tenantId, child);
        SeedUser(ctx, tenantId, grandchild);
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        await service.CreateRelationshipAsync(tenantId, child, root, RelationshipType.Functional, 1);
        await service.CreateRelationshipAsync(tenantId, grandchild, child, RelationshipType.Functional, 1);
        var all = await service.GetAllSubordinatesAsync(tenantId, root);
        Assert.Contains(all, x => x.UserId == child.Value);
        Assert.Contains(all, x => x.UserId == grandchild.Value);
    }

    private static void SeedUser(OroIdentityAppContext ctx, TenantId tenantId, UserId userId)
    {
        var idStr = userId.Value.ToString("N")[..8];
        var user = new OroIdentityServer.Core.Modules.Users.Aggregates.User(
            userId,
            $"Name{idStr}",
            "",
            "Last",
            $"user{idStr}",
            $"user{idStr}@test.com",
            $"ID{idStr}",
            new IdentificationTypeId(Guid.NewGuid()),
            tenantId);
        ctx.Users.Add(user);
    }

    [Fact]
    public async Task CanCommand_ReturnsTrueForSubordinate()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var manager = new UserId(Guid.NewGuid());
        var user = new UserId(Guid.NewGuid());
        ctx.TenantUsers.AddRange(new[] { new TenantUser(tenantId, manager), new TenantUser(tenantId, user) });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);
        await service.CreateRelationshipAsync(tenantId, user, manager, RelationshipType.Functional, 1);
        var can = await service.CanCommandAsync(tenantId, manager, user);
        Assert.True(can);
        var cannot = await service.CanCommandAsync(tenantId, user, manager);
        Assert.False(cannot);
    }

    [Fact]
    public async Task EnforceMaxSuperiors()
    {
        using var ctx = CreateContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        ctx.TenantUsers.Add(new TenantUser(tenantId, userId));
        await ctx.SaveChangesAsync();
        var opts = Options.Create(new HierarchyOptions { MaxDepth = 10, MaxSuperiorsPerUser = 1, MaxSubordinatesPerUser = 1000 });
        var service = new HierarchyService(ctx, opts, NullLogger<HierarchyService>.Instance);
        var sup1 = new UserId(Guid.NewGuid());
        var sup2 = new UserId(Guid.NewGuid());
        await service.CreateRelationshipAsync(tenantId, userId, sup1, RelationshipType.Functional, 1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRelationshipAsync(tenantId, userId, sup2, RelationshipType.Project, 1));
    }
}
