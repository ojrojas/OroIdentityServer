// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using NSubstitute;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class GetUsersByRoleTests
{
    private static User CreateUser(string email = "jane.doe@example.com", string userName = "jane.doe", Guid? tenantId = null) =>
        User.Create(
            userName: userName,
            email: email,
            name: "Jane",
            middleName: "",
            lastName: "Doe",
            identification: Guid.NewGuid().ToString("N"),
            identificationTypeId: IdentificationTypeId.New(null),
            tenantId: tenantId.HasValue ? TenantId.From(tenantId.Value) : TenantId.New());

    private static UserRepository CreateSut(OroIdentityAppContext context)
    {
        var repository = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var securityUserRepository = Substitute.For<ISecurityUserRepository>();
        return new UserRepository(NullLogger<UserRepository>.Instance, repository, securityUserRepository, context);
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_ShouldReturnUsersWithSpecificRole()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var roleId = RoleId.New();

        var userWithRole = CreateUser("with-role@example.com", "with-role");
        userWithRole.AddRole(new UserRole(userWithRole.Id, roleId));

        var userWithoutRole = CreateUser("without-role@example.com", "without-role");
        userWithoutRole.AddRole(new UserRole(userWithoutRole.Id, RoleId.New()));

        context.Users.AddRange(userWithRole, userWithoutRole);
        await context.SaveChangesAsync();

        var result = (await sut.GetUsersByRoleIdAsync(roleId.Value, null, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.Equal(userWithRole.Id.Value, result[0].Id!.Value);
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_ShouldFilterByTenantWhenProvided()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var roleId = RoleId.New();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        var userInTenant = CreateUser("in-tenant@example.com", "in-tenant", tenantId);
        userInTenant.AddRole(new UserRole(userInTenant.Id, roleId));

        var userInOtherTenant = CreateUser("other-tenant@example.com", "other-tenant", otherTenantId);
        userInOtherTenant.AddRole(new UserRole(userInOtherTenant.Id, roleId));

        context.Users.AddRange(userInTenant, userInOtherTenant);
        await context.SaveChangesAsync();

        var result = (await sut.GetUsersByRoleIdAsync(roleId.Value, tenantId, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.Equal(userInTenant.Id.Value, result[0].Id!.Value);
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_ShouldReturnEmptyWhenNoUsersMatch()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var roleId = RoleId.New();

        var user = CreateUser();
        user.AddRole(new UserRole(user.Id, RoleId.New()));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = (await sut.GetUsersByRoleIdAsync(roleId.Value, null, CancellationToken.None)).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_ShouldIncludeRolesInResult()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var roleId = RoleId.New();

        var user = CreateUser("roles-check@example.com", "roles-check");
        user.AddRole(new UserRole(user.Id, roleId));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = (await sut.GetUsersByRoleIdAsync(roleId.Value, null, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.Single(result[0].Roles);
        Assert.Equal(roleId.Value, result[0].Roles.First().RoleId!.Value);
    }
}
