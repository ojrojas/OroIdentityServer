// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

public class TenantSwitchTests
{
    private static TenantRepository CreateRepo(OroIdentityAppContext context)
        => new(NullLogger<TenantRepository>.Instance, new Repository<Tenant>(NullLogger<Repository<Tenant>>.Instance, context));

    [Fact]
    public async Task GetByUserIdAsync_Relational_ReturnsTenantsWhereUserIsMember()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var sut = CreateRepo(context);

        var userId = UserId.New();

        var memberTenant = Tenant.Create("Member Tenant");
        memberTenant.AddUser(userId);

        var otherTenant = Tenant.Create("Other Tenant");
        otherTenant.AddUser(UserId.New());

        context.Tenants.AddRange(memberTenant, otherTenant);
        await context.SaveChangesAsync();

        var result = (await sut.GetByUserIdAsync(userId, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.Equal(memberTenant.Id, result[0].Id);
        Assert.Single(result[0].TenantUsers);
    }

    [Fact]
    public async Task GetByUserIdAsync_Relational_ReturnsHomeTenant_ForSeededAdmin()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var sut = CreateRepo(context);

        var tenant = Tenant.Create("Acme Corp");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userId = UserId.New();
        tenant.AddUser(userId);
        await context.SaveChangesAsync();

        var result = (await sut.GetByUserIdAsync(userId, CancellationToken.None)).ToList();

        var found = Assert.Single(result);
        Assert.Equal(tenant.Id, found.Id);
    }
}
