// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using NSubstitute;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class CreatedTodayCountTests
{
    private static User CreateUser(DateTime createdAt, TenantId tenantId, IdentificationTypeId identificationTypeId, string email)
    {
        var user = User.Create(
            userName: $"count-{Guid.NewGuid():N}",
            email: email,
            name: "Count",
            middleName: "",
            lastName: "User",
            identification: Guid.NewGuid().ToString("N"),
            identificationTypeId: identificationTypeId,
            tenantId: tenantId);

        typeof(User).GetProperty(nameof(User.CreatedAtUtc))!.SetValue(user, createdAt);
        return user;
    }

    [Fact]
    public async Task CountUsersCreatedToday_TenantScopedAndGlobal()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var userRepo = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>(),
            context);

        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);

        var tenantA = Tenant.Create("Tenant A");
        var tenantB = Tenant.Create("Tenant B");
        context.Tenants.AddRange(tenantA, tenantB);
        await context.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        context.Users.AddRange(
            CreateUser(today.AddHours(1), tenantA.Id, identificationType.Id, $"{Guid.NewGuid():N}@example.com"),
            CreateUser(today.AddHours(2), tenantB.Id, identificationType.Id, $"{Guid.NewGuid():N}@example.com"),
            CreateUser(today.AddDays(-1), tenantA.Id, identificationType.Id, $"{Guid.NewGuid():N}@example.com"));
        await context.SaveChangesAsync();

        var tenantACount = await userRepo.CountCreatedTodayAsync(today, tenantA.Id.Value, CancellationToken.None);
        var globalCount = await userRepo.CountCreatedTodayAsync(today, null, CancellationToken.None);

        Assert.Equal(1, tenantACount);
        Assert.Equal(2, globalCount);
    }

    [Fact]
    public async Task CountRolesTenantsIdentificationTypesCreatedToday()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var roleRepo = new RoleRepository(
            NullLogger<RoleRepository>.Instance,
            new Repository<Role>(NullLogger<Repository<Role>>.Instance, context),
            new UserRolesRepository(
                NullLogger<UserRolesRepository>.Instance,
                new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context)));
        var tenantRepo = new TenantRepository(
            NullLogger<TenantRepository>.Instance,
            new Repository<Tenant>(NullLogger<Repository<Tenant>>.Instance, context));
        var identificationRepo = new IdentificationTypeRepository(
            NullLogger<IdentificationTypeRepository>.Instance,
            new Repository<IdentificationType>(NullLogger<Repository<IdentificationType>>.Instance, context));

        var today = DateTime.UtcNow.Date;

        var r1 = new Role(new RoleName("R1"));
        typeof(Role).GetProperty(nameof(Role.CreatedAtUtc))!.SetValue(r1, today.AddHours(1));
        var r2 = new Role(new RoleName("R2"));
        typeof(Role).GetProperty(nameof(Role.CreatedAtUtc))!.SetValue(r2, today.AddDays(-1));
        context.Roles.AddRange(r1, r2);

        var t1 = Tenant.Create("Tenant 1");
        typeof(Tenant).GetProperty(nameof(Tenant.CreatedAtUtc))!.SetValue(t1, today.AddHours(2));
        var t2 = Tenant.Create("Tenant 2");
        typeof(Tenant).GetProperty(nameof(Tenant.CreatedAtUtc))!.SetValue(t2, today.AddDays(-1));
        context.Tenants.AddRange(t1, t2);

        var i1 = IdentificationType.Create("ID1");
        typeof(IdentificationType).GetProperty(nameof(IdentificationType.CreatedAtUtc))!.SetValue(i1, today.AddHours(3));
        var i2 = IdentificationType.Create("ID2");
        typeof(IdentificationType).GetProperty(nameof(IdentificationType.CreatedAtUtc))!.SetValue(i2, today.AddDays(-1));
        context.IdentificationTypes.AddRange(i1, i2);

        await context.SaveChangesAsync();

        Assert.Equal(1, await roleRepo.CountCreatedTodayAsync(today, CancellationToken.None));
        Assert.Equal(1, await tenantRepo.CountCreatedTodayAsync(today, CancellationToken.None));
        Assert.Equal(1, await identificationRepo.CountCreatedTodayAsync(today, CancellationToken.None));
    }
}
