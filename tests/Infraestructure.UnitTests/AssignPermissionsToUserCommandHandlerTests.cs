// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using BuildingBlocks.Kernel.Results;
using NSubstitute;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class AssignPermissionsToUserCommandHandlerTests
{
    private static (AssignPermissionsToUserCommandHandler handler, OroIdentityAppContext context) CreateSut()
    {
        var context = TestDbContextFactory.CreateSqlite();

        var userRepository = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>(),
            context);

        var userPermissionsRepository = new UserPermissionsRepository(
            NullLogger<UserPermissionsRepository>.Instance,
            new Repository<UserPermission>(NullLogger<Repository<UserPermission>>.Instance, context));

        var permissionRepository = new PermissionRepository(
            NullLogger<PermissionRepository>.Instance,
            new Repository<Permission>(NullLogger<Repository<Permission>>.Instance, context),
            context);

        var handler = new AssignPermissionsToUserCommandHandler(
            NullLogger<AssignPermissionsToUserCommandHandler>.Instance,
            userRepository,
            userPermissionsRepository,
            permissionRepository);

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

    private static Permission AddPermission(OroIdentityAppContext context, string action, string resource)
    {
        var permission = Permission.Create("oropos", "desc", action, resource, false);
        context.Permissions.Add(permission);
        return permission;
    }

    [Fact]
    public async Task AssignsDirectPermission()
    {
        var (handler, context) = CreateSut();
        var permission = AddPermission(context, "read", "sales");
        var user = SeedUser(context);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new AssignPermissionsToUserCommand(user.Id.Value, [permission.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var assigned = context.UserPermissions.Where(up => up.UserId == user.Id).ToList();
        Assert.Single(assigned);
        Assert.Equal(permission.Id, assigned[0].PermissionId);
    }

    [Fact]
    public async Task ReplacesSet_AddingAndRemoving()
    {
        var (handler, context) = CreateSut();
        var a = AddPermission(context, "read", "sales");
        var b = AddPermission(context, "write", "sales");
        var c = AddPermission(context, "read", "orders");
        var user = SeedUser(context);
        context.UserPermissions.Add(new UserPermission(user.Id, a.Id));
        context.UserPermissions.Add(new UserPermission(user.Id, b.Id));
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new AssignPermissionsToUserCommand(user.Id.Value, [a.Id.Value, c.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ids = context.UserPermissions.Where(up => up.UserId == user.Id).Select(up => up.PermissionId.Value).ToHashSet();
        Assert.Equal(new HashSet<Guid> { a.Id.Value, c.Id.Value }, ids);
    }

    [Fact]
    public async Task Idempotent_WhenSetUnchanged()
    {
        var (handler, context) = CreateSut();
        var a = AddPermission(context, "read", "sales");
        var user = SeedUser(context);
        context.UserPermissions.Add(new UserPermission(user.Id, a.Id));
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new AssignPermissionsToUserCommand(user.Id.Value, [a.Id.Value]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(context.UserPermissions.Where(up => up.UserId == user.Id));
    }

    [Fact]
    public async Task UnknownUser_ReturnsNotFound()
    {
        var (handler, _) = CreateSut();

        var result = await handler.HandleAsync(
            new AssignPermissionsToUserCommand(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UnknownPermission_ReturnsValidationError()
    {
        var (handler, context) = CreateSut();
        var user = SeedUser(context);
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(
            new AssignPermissionsToUserCommand(user.Id.Value, [Guid.NewGuid()]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
