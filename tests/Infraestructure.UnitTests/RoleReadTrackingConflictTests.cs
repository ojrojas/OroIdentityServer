// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.Data.Sqlite;
using NSubstitute;
using OroIdentityServer.Application.Modules.Roles.Queries;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

/// <summary>
/// Regression: reading a role with its permissions (as the roles API does while validating an
/// assignment) must not attach the Role entity when the scoped context is globally NoTracking.
/// A tracked role would collide with the detached Role instances carried by the user graph that
/// <see cref="AssignRolesToUserCommandHandler"/> attaches via UpdateUserAsync, throwing
/// "another instance with the key value is already being tracked".
/// </summary>
public class RoleReadTrackingConflictTests
{
    private static OroIdentityAppContext CreateNoTrackingSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseSqlite(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
        var context = new OroIdentityAppContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AssignRoles_AfterRoleRead_DoesNotConflict()
    {
        var context = CreateNoTrackingSqlite();

        var roleRepository = new RoleRepository(
            NullLogger<RoleRepository>.Instance,
            new Repository<Role>(NullLogger<Repository<Role>>.Instance, context),
            new UserRolesRepository(
                NullLogger<UserRolesRepository>.Instance,
                new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context)),
            context);
        var userRolesRepository = new UserRolesRepository(
            NullLogger<UserRolesRepository>.Instance,
            new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context));
        var userRepository = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>(),
            context);

        // Seed: user with role1 assigned, plus role2.
        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);
        var tenant = Tenant.Create($"Tenant-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var user = User.Create(
            $"user-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        context.Users.Add(user);

        var role1 = new Role(new RoleName($"Role1-{Guid.NewGuid():N}"));
        var role2 = new Role(new RoleName($"Role2-{Guid.NewGuid():N}"));
        context.Roles.AddRange(role1, role2);
        context.UserRoles.Add(new UserRole(user.Id, role1.Id));
        await context.SaveChangesAsync();

        // The roles API validates the requested roles through GetRoleByIdQuery before assigning.
        var roleQuery = new GetRoleByIdQueryHandler(
            NullLogger<GetRoleByIdQueryHandler>.Instance, roleRepository);
        await roleQuery.HandleAsync(new GetRoleByIdQuery(role1.Id.Value), CancellationToken.None);
        await roleQuery.HandleAsync(new GetRoleByIdQuery(role2.Id.Value), CancellationToken.None);

        var handler = new AssignRolesToUserCommandHandler(
            NullLogger<AssignRolesToUserCommandHandler>.Instance, userRepository, userRolesRepository);

        var result = await handler.HandleAsync(
            new AssignRolesToUserCommand(user.Id.Value, [role1.Id.Value, role2.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
