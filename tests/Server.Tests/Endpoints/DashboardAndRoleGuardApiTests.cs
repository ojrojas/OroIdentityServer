// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using IdentityServer.Client.Models.Dashboard;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

[Collection(nameof(AspireTestCollection))]
public sealed class DashboardAndRoleGuardApiTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";

    private async Task<(HttpClient client, Guid tenantId)> LoginAsAdminAsync()
    {
        await using var context = app.CreateDbContext();
        var passwordHasher = app.PasswordHasher;

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (identificationType is null)
        {
            identificationType = IdentificationType.Create("Passport");
            context.IdentificationTypes.Add(identificationType);
        }

        var tenant = context.Tenants.AsEnumerable().First(t => t.Name.Value == "OroMasterTenant");

        var userName = $"admin-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

        var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(Password));
        securityUser.ExemptFromPasswordChange();
        context.SecurityUsers.Add(securityUser);
        user.AssignSecurityUser(securityUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        tenant.AddUser(user.Id);
        await context.SaveChangesAsync();

        // Mirror the master-admin shape: catalogue Administrator + master tenant.
        var adminRole = context.Roles.AsEnumerable().FirstOrDefault(r => r.Name.Value == "Administrator");
        if (adminRole is null)
        {
            adminRole = new Role(new RoleName("Administrator"));
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();
        }
        context.UserRoles.Add(new UserRole(user.Id, adminRole.Id));
        await context.SaveChangesAsync();

        var client = app.CreateClient();
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = userName,
            ["password"] = Password
        });

        var response = await client.PostAsync("/auth/login", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return (client, tenant.Id.Value);
    }

    [Fact]
    public async Task GetDashboardStats_ReflectsSeededDeltas()
    {
        var (client, _) = await LoginAsAdminAsync();

        var baseline = await client.GetFromJsonAsync<DashboardStatsModel>("/api/dashboard/stats");
        Assert.NotNull(baseline);

        await using (var context = app.CreateDbContext())
        {
            var identificationType = IdentificationType.Create($"StatsID-{Guid.NewGuid():N}");
            context.IdentificationTypes.Add(identificationType);

            var tenant = Tenant.Create($"Tenant-Stats-{Guid.NewGuid():N}");
            context.Tenants.Add(tenant);

            var role = new Role(new RoleName($"Role-Stats-{Guid.NewGuid():N}"));
            context.Roles.Add(role);

            var user = User.Create(
                $"stats-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Stats", "", "User",
                Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
            context.Users.Add(user);

            await context.SaveChangesAsync();
        }

        var after = await client.GetFromJsonAsync<DashboardStatsModel>("/api/dashboard/stats");
        Assert.NotNull(after);

        Assert.Equal(baseline.UsersCreatedToday + 1, after.UsersCreatedToday);
        Assert.Equal(baseline.RolesCreatedToday + 1, after.RolesCreatedToday);
        Assert.Equal(baseline.TenantsCreatedToday + 1, after.TenantsCreatedToday);
        Assert.Equal(baseline.IdentificationTypesCreatedToday + 1, after.IdentificationTypesCreatedToday);
        Assert.Equal(baseline.ConnectedUsers, after.ConnectedUsers);
    }

    [Fact]
    public async Task DeleteRole_AssignedToUser_ReturnsConflict()
    {
        var (client, _) = await LoginAsAdminAsync();
        var (roleId, userId) = await SeedRoleWithUserAsync();

        var response = await client.DeleteAsync($"/api/roles/{roleId}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRole_WithPermissions_ReturnsConflict()
    {
        var (client, _) = await LoginAsAdminAsync();
        var roleId = await SeedRoleWithPermissionAsync();

        var response = await client.DeleteAsync($"/api/roles/{roleId}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardStats_TenantScoped_CountsOnlyTenantUsers()
    {
        var (client, tenantId) = await LoginAsAdminAsync();

        // Baseline scoped to this tenant: the seeded master-tenant admin (and any other
        // users created today in the master tenant by the seeder or other tests) count here.
        var baselineRequest = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/stats");
        baselineRequest.Headers.Add("X-Tenant-Id", tenantId.ToString());
        var baselineResponse = await client.SendAsync(baselineRequest);
        baselineResponse.EnsureSuccessStatusCode();
        var baseline = await baselineResponse.Content.ReadFromJsonAsync<DashboardStatsModel>();
        Assert.NotNull(baseline);

        var globalBaseline = await client.GetFromJsonAsync<DashboardStatsModel>("/api/dashboard/stats");
        Assert.NotNull(globalBaseline);

        // Seed a user created today in a DIFFERENT tenant: it must NOT count for this tenant.
        await using (var context = app.CreateDbContext())
        {
            var identificationType = context.IdentificationTypes
                .AsEnumerable()
                .First(i => i.Name.Value == "Passport");
            var otherTenant = Tenant.Create($"Other-{Guid.NewGuid():N}");
            context.Tenants.Add(otherTenant);
            var user = User.Create(
                $"other-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Other", "", "User",
                Guid.NewGuid().ToString("N"), identificationType.Id, otherTenant.Id);
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var afterRequest = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/stats");
        afterRequest.Headers.Add("X-Tenant-Id", tenantId.ToString());
        var afterResponse = await client.SendAsync(afterRequest);
        afterResponse.EnsureSuccessStatusCode();
        var stats = await afterResponse.Content.ReadFromJsonAsync<DashboardStatsModel>();
        Assert.NotNull(stats);

        // The other-tenant user is excluded from the scoped count but included globally.
        Assert.Equal(baseline.UsersCreatedToday, stats.UsersCreatedToday);

        var globalAfter = await client.GetFromJsonAsync<DashboardStatsModel>("/api/dashboard/stats");
        Assert.NotNull(globalAfter);
        Assert.Equal(globalBaseline.UsersCreatedToday + 1, globalAfter.UsersCreatedToday);
    }

    [Fact]
    public async Task DeleteRole_Clean_ReturnsSuccess()
    {
        var (client, _) = await LoginAsAdminAsync();

        Guid roleId;
        await using (var context = app.CreateDbContext())
        {
            var role = new Role(new RoleName($"Role-Clean-{Guid.NewGuid():N}"));
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            roleId = role.Id.Value;
        }

        var response = await client.DeleteAsync($"/api/roles/{roleId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<(Guid roleId, Guid userId)> SeedRoleWithUserAsync()
    {
        await using var context = app.CreateDbContext();

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (identificationType is null)
        {
            identificationType = IdentificationType.Create("Passport");
            context.IdentificationTypes.Add(identificationType);
        }

        var tenant = Tenant.Create($"Tenant-RoleUser-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var role = new Role(new RoleName($"Role-Guard-{Guid.NewGuid():N}"));
        context.Roles.Add(role);

        var user = User.Create(
            $"guard-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Guard", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        context.Users.Add(user);

        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        return (role.Id.Value, user.Id.Value);
    }

    private async Task<Guid> SeedRoleWithPermissionAsync()
    {
        await using var context = app.CreateDbContext();

        var role = new Role(new RoleName($"Role-Perm-{Guid.NewGuid():N}"));
        var permission = Permission.Create("System", "desc", "*", "*", true);
        context.Permissions.Add(permission);
        role.AddPermission(permission);
        context.Roles.Add(role);

        await context.SaveChangesAsync();
        return role.Id.Value;
    }
}
