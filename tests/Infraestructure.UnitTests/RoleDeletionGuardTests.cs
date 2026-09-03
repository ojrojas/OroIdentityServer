// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using BuildingBlocks.Kernel.Results;
using OroIdentityServer.Application.Modules.Roles.Commands;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class RoleDeletionGuardTests
{
    private static (DeleteRoleCommandHandler handler, OroIdentityAppContext context) CreateSut()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var roleRepo = new RoleRepository(
            NullLogger<RoleRepository>.Instance,
            new Repository<Role>(NullLogger<Repository<Role>>.Instance, context),
            new UserRolesRepository(
                NullLogger<UserRolesRepository>.Instance,
                new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context)));
        var userRolesRepo = new UserRolesRepository(
            NullLogger<UserRolesRepository>.Instance,
            new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context));

        var handler = new DeleteRoleCommandHandler(
            roleRepo,
            userRolesRepo,
            NullLogger<DeleteRoleCommandHandler>.Instance);

        return (handler, context);
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

    [Fact]
    public async Task DeleteRole_AssignedToUser_ReturnsConflict()
    {
        var (handler, context) = CreateSut();

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var user = SeedUser(context);
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task DeleteRole_WithPermissions_ReturnsConflict()
    {
        var (handler, context) = CreateSut();

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        var permission = Permission.Create("System", "desc", "*", "*", true);
        context.Permissions.Add(permission);
        role.AddPermission(permission);
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task DeleteRole_NoAssociations_SoftDeletes()
    {
        var (handler, context) = CreateSut();

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var reloaded = await context.Roles
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == role.Id);
        Assert.NotNull(reloaded);
        Assert.False(reloaded.IsActive);
    }
}
