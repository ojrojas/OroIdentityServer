// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using NSubstitute;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Entities;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class CreateUserTenantMembershipTests
{
    [Fact]
    public async Task CreateUser_CreatesTenantMembership_InConfiguredTenant()
    {
        var context = TestDbContextFactory.CreateSqlite();

        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);

        var tenant = Tenant.Create($"Tenant-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userRepo = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>());
        var tenantRepo = new TenantRepository(
            NullLogger<TenantRepository>.Instance,
            new Repository<Tenant>(NullLogger<Repository<Tenant>>.Instance, context));

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");

        var tenantUserRepo = new Repository<TenantUser>(NullLogger<Repository<TenantUser>>.Instance, context);

        var handler = new CreateUserCommandHandler(
            NullLogger<CreateUserCommandHandler>.Instance,
            userRepo,
            tenantRepo,
            tenantUserRepo,
            passwordHasher);

        var command = new CreateUserCommand(
            "Test", "", "User", $"create-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com",
            "Abc123456#", Guid.NewGuid().ToString("N"), identificationType.Id.Value, tenant.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var createdUser = context.Users.Single(u => u.Email == command.Email);
        var memberships = context.TenantUsers.Where(tu => tu.UserId == createdUser.Id).ToList();

        Assert.Single(memberships);
        Assert.Equal(tenant.Id, memberships[0].TenantId);
        Assert.Equal(TenantRole.Member, memberships[0].Role);
    }
}
