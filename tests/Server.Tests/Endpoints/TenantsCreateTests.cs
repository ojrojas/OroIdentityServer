// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Integration tests (Aspire.Hosting.Testing) for tenant creation via POST /api/tenants/
/// (MasterAdminOnly), executed against the full Aspire AppHost.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class TenantsCreateTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string Password = "Abc123456#";

    [Fact]
    public async Task CreateTenant_WithMasterAdminSession_ReturnsCreated()
    {
        var client = await CreateLoggedInClientAsync();

        var response = await client.PostAsJsonAsync("/api/tenants/", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithNonMasterAdminSession_ReturnsForbidden()
    {
        var client = await CreateLoggedInNonMasterAdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/tenants/", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<HttpClient> CreateLoggedInClientAsync()
    {
        var client = app.CreateClient();
        var login = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = AdminUser,
                ["password"] = AdminPassword
            }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        return client;
    }

    /// <summary>
    /// Seeds an Administrator-catalogue user in a non-master tenant (so they are not the
    /// master admin) and logs them in; /api/tenants writes must still be forbidden.
    /// </summary>
    private async Task<HttpClient> CreateLoggedInNonMasterAdminClientAsync()
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

        var tenant = Tenant.Create($"Other-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var userName = $"creator-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

        var securityUser = SecurityUser.Create(await app.PasswordHasher.HashPassword(Password));
        securityUser.ExemptFromPasswordChange();
        context.SecurityUsers.Add(securityUser);
        user.AssignSecurityUser(securityUser);
        context.Users.Add(user);
        tenant.AddUser(user.Id);
        await context.SaveChangesAsync();

        var role = context.Roles.AsEnumerable().FirstOrDefault(r => r.Name.Value == "Administrator");
        if (role is null)
        {
            role = new Role(new RoleName("Administrator"));
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var client = app.CreateClient();
        var login = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = userName,
                ["password"] = Password
            }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        return client;
    }
}
