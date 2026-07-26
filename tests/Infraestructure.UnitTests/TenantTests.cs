// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class TenantTests
{
    [Theory]
    [InlineData(TenantRole.Admin)]
    [InlineData(TenantRole.Manager)]
    [InlineData(TenantRole.Member)]
    public void AddUser_ShouldAccept_ValidTenantRoles(string role)
    {
        var tenant = Tenant.Create("Acme Corp");

        var tenantUser = tenant.AddUser(UserId.New(), role);

        Assert.Equal(role, tenantUser.Role);
    }

    [Fact]
    public void AddUser_ShouldReject_InvalidRole()
    {
        var tenant = Tenant.Create("Acme Corp");

        var act = () => tenant.AddUser(UserId.New(), "SuperAdmin");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void AddUser_ShouldReject_DuplicateMembership()
    {
        var tenant = Tenant.Create("Acme Corp");
        var userId = UserId.New();
        tenant.AddUser(userId, TenantRole.Member);

        var act = () => tenant.AddUser(userId, TenantRole.Admin);

        Assert.Throws<InvalidOperationException>(act);
    }
}
