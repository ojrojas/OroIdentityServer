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

public class SetRolePermissionsCommandHandlerTests
{
    private static (SetRolePermissionsCommandHandler handler, OroIdentityAppContext context) CreateSut()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var roleRepo = new RoleRepository(
            NullLogger<RoleRepository>.Instance,
            new Repository<Role>(NullLogger<Repository<Role>>.Instance, context),
            new UserRolesRepository(
                NullLogger<UserRolesRepository>.Instance,
                new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context)));
        var permissionRepo = new PermissionRepository(
            NullLogger<PermissionRepository>.Instance,
            new Repository<Permission>(NullLogger<Repository<Permission>>.Instance, context),
            context);

        var handler = new SetRolePermissionsCommandHandler(
            NullLogger<SetRolePermissionsCommandHandler>.Instance, roleRepo, permissionRepo);
        return (handler, context);
    }

    private static Permission AddPermission(OroIdentityAppContext context, string action, string resource)
    {
        var permission = Permission.Create("oropos", "desc", action, resource, false);
        context.Permissions.Add(permission);
        return permission;
    }

    [Fact]
    public async Task ReplacesSet_AddingAndRemoving()
    {
        var (handler, context) = CreateSut();
        var a = AddPermission(context, "read", "sales");
        var b = AddPermission(context, "write", "sales");
        var c = AddPermission(context, "read", "orders");

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        role.AddPermission(a);
        role.AddPermission(b);
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new SetRolePermissionsCommand(role.Id.Value, [a.Id.Value, c.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var reloaded = await context.Roles
            .Include(r => r.RolePermissions)
            .SingleAsync(r => r.Id == role.Id);
        var ids = reloaded.RolePermissions.Select(rp => rp.PermissionId.Value).ToHashSet();
        Assert.Equal(new HashSet<Guid> { a.Id.Value, c.Id.Value }, ids);
    }

    [Fact]
    public async Task Idempotent_WhenSetUnchanged()
    {
        var (handler, context) = CreateSut();
        var a = AddPermission(context, "read", "sales");

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        role.AddPermission(a);
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new SetRolePermissionsCommand(role.Id.Value, [a.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = await context.Roles
            .Include(r => r.RolePermissions)
            .SingleAsync(r => r.Id == role.Id);
        Assert.Single(reloaded.RolePermissions);
    }

    [Fact]
    public async Task UnknownRole_ReturnsNotFound()
    {
        var (handler, _) = CreateSut();

        var result = await handler.HandleAsync(
            new SetRolePermissionsCommand(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UnknownPermission_ReturnsValidationError()
    {
        var (handler, context) = CreateSut();
        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new SetRolePermissionsCommand(role.Id.Value, [Guid.NewGuid()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
