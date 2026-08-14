// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

public class TenantTests
{
    [Fact]
    public void AddUser_ShouldAdd_Membership()
    {
        var tenant = Tenant.Create("Acme Corp");
        var userId = UserId.New();

        var tenantUser = tenant.AddUser(userId);

        Assert.Equal(userId, tenantUser.UserId);
        Assert.True(tenantUser.IsActive);
    }

    [Fact]
    public void AddUser_ShouldReject_DuplicateMembership()
    {
        var tenant = Tenant.Create("Acme Corp");
        var userId = UserId.New();
        tenant.AddUser(userId);

        var act = () => tenant.AddUser(userId);

        Assert.Throws<InvalidOperationException>(act);
    }
}
