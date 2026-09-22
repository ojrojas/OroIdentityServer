// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class PermissionResolutionTests
{
    private static (PermissionRepository repository, OroIdentityAppContext context) CreateSut()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var repository = new PermissionRepository(
            NullLogger<PermissionRepository>.Instance,
            new Repository<Permission>(NullLogger<Repository<Permission>>.Instance, context),
            context);
        return (repository, context);
    }

    private static User SeedUser(OroIdentityAppContext context)
    {
        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);

        var tenant = Tenant.Create($"Tenant-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var user = User.Create(
            $"user-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Role SeedRoleWithPermission(OroIdentityAppContext context, Permission permission, bool active = true)
    {
        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        role.AddPermission(permission);
        if (!active) role.Deactivate();
        context.Roles.Add(role);
        return role;
    }

    [Fact]
    public async Task ReturnsDistinctNamesForActiveRoles()
    {
        var (repository, context) = CreateSut();

        var permission = Permission.Create("oropos", "Read sales", "read", "sales", false);
        context.Permissions.Add(permission);
        var role = SeedRoleWithPermission(context, permission);
        var user = SeedUser(context);
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var names = await repository.GetPermissionNamesByUserIdAsync(user.Id, CancellationToken.None);

        Assert.Equal(["oropos.sales.read"], names);
    }

    [Fact]
    public async Task DeactivatedRoleIsExcluded()
    {
        var (repository, context) = CreateSut();

        var permission = Permission.Create("oropos", "Read sales", "read", "sales", false);
        context.Permissions.Add(permission);
        var role = SeedRoleWithPermission(context, permission, active: false);
        var user = SeedUser(context);
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var names = await repository.GetPermissionNamesByUserIdAsync(user.Id, CancellationToken.None);

        Assert.Empty(names);
    }

    [Fact]
    public async Task UserWithoutRolesReturnsEmpty()
    {
        var (repository, context) = CreateSut();
        var user = SeedUser(context);

        var names = await repository.GetPermissionNamesByUserIdAsync(user.Id, CancellationToken.None);

        Assert.Empty(names);
    }
}
