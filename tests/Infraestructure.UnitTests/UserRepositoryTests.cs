// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using NSubstitute;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class UserRepositoryTests
{
    private static User CreateUser(string email = "jane.doe@example.com", string userName = "jane.doe") =>
        User.Create(
            userName: userName,
            email: email,
            name: "Jane",
            middleName: "",
            lastName: "Doe",
            identification: Guid.NewGuid().ToString("N"),
            identificationTypeId: IdentificationTypeId.New(null),
            tenantId: TenantId.New());

    private static UserRepository CreateSut(OroIdentityAppContext context)
    {
        var repository = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var securityUserRepository = Substitute.For<ISecurityUserRepository>();
        return new UserRepository(NullLogger<UserRepository>.Instance, repository, securityUserRepository, context);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldIncludeRoles()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);

        var user = CreateUser();
        user.AddRole(new UserRole(user.Id, RoleId.New()));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await sut.GetUserByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Roles);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldIncludeRolesForEveryUser()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);

        var userWithRole = CreateUser("with-role@example.com", "with-role");
        userWithRole.AddRole(new UserRole(userWithRole.Id, RoleId.New()));

        var userWithoutRole = CreateUser("without-role@example.com", "without-role");

        context.Users.AddRange(userWithRole, userWithoutRole);
        await context.SaveChangesAsync();

        var result = (await sut.GetAllUsersAsync(CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Single(result.Single(u => u.Id == userWithRole.Id).Roles);
        Assert.Empty(result.Single(u => u.Id == userWithoutRole.Id).Roles);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldIncludeRoles()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);

        var user = CreateUser("email-lookup@example.com", "email-lookup");
        user.AddRole(new UserRole(user.Id, RoleId.New()));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await sut.GetUserByEmailAsync("email-lookup@example.com", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Roles);
    }
}
