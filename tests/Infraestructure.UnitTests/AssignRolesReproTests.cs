// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using NSubstitute;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class AssignRolesReproTests
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

    private static (UserRepository userRepo, UserRolesRepository roleRepo) CreateRepos(OroIdentityAppContext context)
    {
        var userRepo = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>());

        var roleRepo = new UserRolesRepository(
            NullLogger<UserRolesRepository>.Instance,
            new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context));

        return (userRepo, roleRepo);
    }

    private static User SeedUser(OroIdentityAppContext context)
    {
        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);

        var tenant = Tenant.Create($"Tenant-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var user = User.Create(
            userName: "jane.doe",
            email: "jane.doe@example.com",
            name: "Jane",
            middleName: "",
            lastName: "Doe",
            identification: Guid.NewGuid().ToString("N"),
            identificationTypeId: identificationType.Id,
            tenantId: tenant.Id);
        user.AssignSecurityUser(SecurityUser.Create("hash"));

        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static RoleId SeedRole(OroIdentityAppContext context)
    {
        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        context.SaveChanges();
        return role.Id;
    }

    [Fact]
    public async Task AssignRoles_AddNewRole_ShouldNotThrowTrackingConflict()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var (userRepo, roleRepo) = CreateRepos(context);

        var user = SeedUser(context);
        var roleId = SeedRole(context);

        var loaded = await userRepo.GetUserByIdAsync(user.Id, CancellationToken.None);
        Assert.NotNull(loaded);

        var newRole = new UserRole(user.Id, roleId);
        loaded!.AddRole(newRole);
        await roleRepo.AddUserRoleAsync(newRole, CancellationToken.None);

        await userRepo.UpdateUserAsync(loaded, CancellationToken.None);
    }

    [Fact]
    public async Task AssignRoles_AddAndRemove_ShouldNotThrowTrackingConflict()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var (userRepo, roleRepo) = CreateRepos(context);

        var user = SeedUser(context);
        var roleAId = SeedRole(context);
        var roleBId = SeedRole(context);
        var roleCId = SeedRole(context);

        var roleA = new UserRole(user.Id, roleAId);
        var roleB = new UserRole(user.Id, roleBId);
        user.AddRole(roleA);
        user.AddRole(roleB);
        context.UserRoles.AddRange(roleA, roleB);
        await context.SaveChangesAsync();

        var loaded = await userRepo.GetUserByIdAsync(user.Id, CancellationToken.None);
        Assert.NotNull(loaded);

        var currentRoles = (await roleRepo.GetRolesByUserIdAsync(user.Id, CancellationToken.None)).ToList();

        var toRemove = currentRoles.Where(r => r.RoleId!.Value == roleAId.Value).ToList();
        foreach (var role in toRemove)
        {
            loaded!.RemoveRole(role);
            await roleRepo.DeleteUserRoleAsync(role, CancellationToken.None);
        }

        var newRole = new UserRole(user.Id, roleCId);
        loaded!.AddRole(newRole);
        await roleRepo.AddUserRoleAsync(newRole, CancellationToken.None);

        await userRepo.UpdateUserAsync(loaded, CancellationToken.None);
    }
}
