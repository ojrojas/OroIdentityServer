// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.Data.Sqlite;
using OroIdentityServer.Application.Modules.Roles.Commands;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;

namespace OroIdentityServer.Infraestructure.UnitTests;

/// <summary>
/// Reproduces the real request lifecycle: the role is seeded with one DbContext and the
/// command handler runs with a different DbContext over the same database.
/// </summary>
public class SetRolePermissionsPersistenceTests
{
    private static (DbContextOptions<OroIdentityAppContext> options, SqliteConnection connection) CreateSharedSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseSqlite(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
        using (var ctx = new OroIdentityAppContext(options))
        {
            ctx.Database.EnsureCreated();
        }
        return (options, connection);
    }

    private static SetRolePermissionsCommandHandler CreateHandler(OroIdentityAppContext context)
    {
        var roleRepo = new RoleRepository(
            NullLogger<RoleRepository>.Instance,
            new Repository<Role>(NullLogger<Repository<Role>>.Instance, context),
            new UserRolesRepository(
                NullLogger<UserRolesRepository>.Instance,
                new Repository<OroIdentityServer.Core.Modules.Users.Entities.UserRole>(NullLogger<Repository<OroIdentityServer.Core.Modules.Users.Entities.UserRole>>.Instance, context)),
            context);
        var permissionRepo = new PermissionRepository(
            NullLogger<PermissionRepository>.Instance,
            new Repository<Permission>(NullLogger<Repository<Permission>>.Instance, context),
            context);
        return new SetRolePermissionsCommandHandler(
            NullLogger<SetRolePermissionsCommandHandler>.Instance, roleRepo, permissionRepo);
    }

    [Fact]
    public async Task AssignPermission_FromDifferentContext_Persists()
    {
        var (options, connection) = CreateSharedSqlite();
        await using var _ = connection;

        Guid roleId;
        Guid permissionId;
        await using (var seed = new OroIdentityAppContext(options))
        {
            var permission = Permission.Create("oropos", "Read sales", "read", "sales", false);
            seed.Permissions.Add(permission);
            var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
            seed.Roles.Add(role);
            await seed.SaveChangesAsync();
            roleId = role.Id.Value;
            permissionId = permission.Id.Value;
        }

        await using (var handlerContext = new OroIdentityAppContext(options))
        {
            var handler = CreateHandler(handlerContext);
            var result = await handler.HandleAsync(
                new SetRolePermissionsCommand(roleId, [permissionId]), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        await using (var verify = new OroIdentityAppContext(options))
        {
            var role = await verify.Roles
                .Include(r => r.RolePermissions)
                .SingleAsync(r => r.Id == new RoleId(roleId));
            Assert.Contains(role.RolePermissions, rp => rp.PermissionId.Value == permissionId);
        }
    }
}
