// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class SecurityUserChangePasswordTests
{
    private static SecurityUserRepository CreateRepo(OroIdentityAppContext context)
        => new(
            NullLogger<SecurityUserRepository>.Instance,
            new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context));

    [Fact]
    public async Task ChangePassword_LoadedViaFind_ThenUpdated_DoesNotThrowConcurrency()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var repo = CreateRepo(context);

        var securityUser = SecurityUser.Create("hash");
        context.SecurityUsers.Add(securityUser);
        await context.SaveChangesAsync();

        // Mimic the change-password endpoint: load via FindAsync (tracked), then update.
        var loaded = await repo.GetSecurityUserAsync(securityUser.Id.Value, CancellationToken.None);
        Assert.NotNull(loaded);

        loaded.ChangePassword("new-hash");

        await repo.UpdateSecurityUserAsync(loaded, CancellationToken.None);

        var reloaded = await context.SecurityUsers
            .AsNoTracking()
            .SingleAsync(s => s.Id == securityUser.Id);
        Assert.Equal("new-hash", reloaded.PasswordHash);
        Assert.False(reloaded.MustChangePassword);
    }

    [Fact]
    public async Task ChangePassword_NoTrackingContext_UserIncludedThenFind_ThenUpdated_DoesNotThrowConcurrency()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var context = new OroIdentityAppContext(
            new DbContextOptionsBuilder<OroIdentityAppContext>()
                .UseSqlite(connection)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options);
        context.Database.EnsureCreated();

        var identificationType = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(identificationType);
        var tenant = Tenant.Create("Tenant");
        context.Tenants.Add(tenant);

        var user = User.Create(
            $"chpw-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "T", "", "U",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        user.AssignSecurityUser(SecurityUser.Create("hash"));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userRepo = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            CreateRepo(context),
            context);

        var loadedUser = await userRepo.GetUserByIdAsync(user.Id, CancellationToken.None);
        Assert.NotNull(loadedUser);

        var securityRepo = CreateRepo(context);
        var securityUser = await securityRepo.GetSecurityUserAsync(loadedUser.SecurityUserId!.Value, CancellationToken.None);
        Assert.NotNull(securityUser);

        securityUser.ChangePassword("new-hash");
        await securityRepo.UpdateSecurityUserAsync(securityUser, CancellationToken.None);

        var reloaded = await context.SecurityUsers
            .AsNoTracking()
            .SingleAsync(s => s.Id == securityUser.Id);
        Assert.Equal("new-hash", reloaded.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_StaleConcurrencyToken_RetriesSuccessfully()
    {
        var context = TestDbContextFactory.CreateSqlite();
        var repo = CreateRepo(context);

        var securityUser = SecurityUser.Create("hash");
        context.SecurityUsers.Add(securityUser);
        await context.SaveChangesAsync();

        // Load into the tracking context (original SecurityStamp captured).
        var loaded = await repo.GetSecurityUserAsync(securityUser.Id.Value, CancellationToken.None);
        Assert.NotNull(loaded);

        // Simulate the DB row changing after the load: the concurrency token no longer matches.
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE \"SecurityUsers\" SET \"SecurityStamp\" = {0} WHERE \"Id\" = {1}",
            Guid.NewGuid().ToString(), securityUser.Id.Value);

        // ChangePassword sets a new SecurityStamp; the first SaveChanges hits the concurrency
        // conflict and the repository retries with the refreshed original.
        loaded.ChangePassword("new-hash");
        await repo.UpdateSecurityUserAsync(loaded, CancellationToken.None);

        var reloaded = await context.SecurityUsers
            .AsNoTracking()
            .SingleAsync(s => s.Id == securityUser.Id);
        Assert.Equal("new-hash", reloaded.PasswordHash);
        Assert.False(reloaded.MustChangePassword);
    }
}
