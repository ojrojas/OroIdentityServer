// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Verifies that admin-console authorization (Admin vs. Manager tenant role) is actually enforced by
/// the API, not just hidden in the UI - the test provisions its own tenant/user/role directly against
/// the database shared with the Aspire-hosted server.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class AdminApiRoleAuthorizationTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";

    private async Task<HttpClient> LoginAsAsync(string tenantRole)
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

        var tenant = Tenant.Create($"Tenant-{tenantRole}-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userName = $"{tenantRole.ToLowerInvariant()}-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

        var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(Password));
        securityUser.ExemptFromPasswordChange();
        context.SecurityUsers.Add(securityUser);
        user.AssignSecurityUser(securityUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        tenant.AddUser(user.Id, tenantRole);
        await context.SaveChangesAsync();

        var client = app.CreateClient();
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = userName,
            ["password"] = Password
        });

        var response = await client.PostAsync("/auth/login", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return client;
    }

    [Fact]
    public async Task Manager_CannotCreateTenant()
    {
        var client = await LoginAsAsync(TenantRole.Manager);

        var response = await client.PostAsJsonAsync("/api/tenants", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CanListUsers()
    {
        var client = await LoginAsAsync(TenantRole.Manager);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateTenant()
    {
        var client = await LoginAsAsync(TenantRole.Admin);

        var response = await client.PostAsJsonAsync("/api/tenants", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
