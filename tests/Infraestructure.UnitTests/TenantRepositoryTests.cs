// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class TenantRepositoryTests
{
    private static (TenantRepository TenantRepository, OroIdentityAppContext Context, string DatabaseName) CreateSut()
    {
        var databaseName = Guid.NewGuid().ToString();
        var context = new OroIdentityAppContext(
            new DbContextOptionsBuilder<OroIdentityAppContext>()
                .UseInMemoryDatabase(databaseName)
                .Options);
        var repository = new Repository<Tenant>(NullLogger<Repository<Tenant>>.Instance, context);
        var tenantRepository = new TenantRepository(NullLogger<TenantRepository>.Instance, repository);
        return (tenantRepository, context, databaseName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeTenantUsers()
    {
        var (sut, context, _) = CreateSut();

        var tenant = Tenant.Create("Acme Corp");
        var userId = UserId.New();
        tenant.AddUser(userId, TenantRole.Admin);

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var result = await sut.GetByIdAsync(tenant.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.TenantUsers);
        Assert.Equal(userId, result.TenantUsers.Single().UserId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeTenantUsersForEveryTenant()
    {
        var (sut, context, _) = CreateSut();

        var tenantWithUsers = Tenant.Create("Tenant With Users");
        tenantWithUsers.AddUser(UserId.New(), TenantRole.Admin);
        tenantWithUsers.AddUser(UserId.New(), TenantRole.Member);

        var tenantWithoutUsers = Tenant.Create("Tenant Without Users");

        context.Tenants.AddRange(tenantWithUsers, tenantWithoutUsers);
        await context.SaveChangesAsync();

        var result = (await sut.GetAllAsync(CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Single(t => t.Id == tenantWithUsers.Id).TenantUsers.Count);
        Assert.Empty(result.Single(t => t.Id == tenantWithoutUsers.Id).TenantUsers);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyTenantsWhereUserIsActiveMember_WithTenantUsersIncluded()
    {
        var (sut, context, _) = CreateSut();

        var targetUserId = UserId.New();

        var memberTenant = Tenant.Create("Member Tenant");
        memberTenant.AddUser(targetUserId, TenantRole.Admin);
        memberTenant.AddUser(UserId.New(), TenantRole.Member);

        var otherTenant = Tenant.Create("Other Tenant");
        otherTenant.AddUser(UserId.New(), TenantRole.Admin);

        context.Tenants.AddRange(memberTenant, otherTenant);
        await context.SaveChangesAsync();

        var result = (await sut.GetByUserIdAsync(targetUserId, CancellationToken.None)).ToList();

        var tenant = Assert.Single(result);
        Assert.Equal(memberTenant.Id, tenant.Id);
        Assert.Equal(2, tenant.TenantUsers.Count);
    }

    [Fact]
    public async Task AddUser_ShouldPersistTenantUser_AfterUpdate()
    {
        var (sut, context, databaseName) = CreateSut();

        var tenant = Tenant.Create("Fresh Tenant");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userId = UserId.New();
        tenant.AddUser(userId, TenantRole.Admin);
        await sut.UpdateAsync(tenant, CancellationToken.None);

        using var verificationContext = new OroIdentityAppContext(
            new DbContextOptionsBuilder<OroIdentityAppContext>()
                .UseInMemoryDatabase(databaseName)
                .Options);

        var persisted = await verificationContext.Tenants
            .Include(t => t.TenantUsers)
            .FirstAsync(t => t.Id == tenant.Id);

        Assert.Contains(persisted.TenantUsers, tu => tu.UserId == userId);
    }
}
