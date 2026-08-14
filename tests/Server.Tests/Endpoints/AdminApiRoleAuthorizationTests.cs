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
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Verifies the catalogue-only role model:
///   * ManagerOrAdmin policy: Admin/Administrator/Manager can call /api endpoints.
///   * AdminOnly policy: Admin/Administrator only (Manager is denied).
///   * MasterAdminOnly policy: only the master admin (catalogue "Administrator" AND
///     User.TenantId == SEED_TENANT_NAME) can manage tenants, OIDC applications and scopes.
///   * Non-master "Administrator" catalogue members are app admins: they can read users
///     but cannot create tenants or touch the OIDC catalogue.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class AdminApiRoleAuthorizationTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";
    private const string MasterTenantName = "OroMasterTenant";
    private const string CatalogueAdministrator = "Administrator";
    private const string CatalogueManager = "Manager";

    private static Tenant GetMasterTenant(OroIdentityAppContext context)
        => context.Tenants.AsEnumerable().First(t => t.Name.Value == MasterTenantName);

    /// <summary>
    /// Creates a user in the master tenant, assigns the given catalogue role (or none), and
    /// logs them in. The user's effective role claim is derived from the catalogue role, so
    /// this is the only knob the tests need to turn to model the different personas.
    /// </summary>
    private async Task<HttpClient> LoginInMasterTenantAsync(string? catalogueRoleName = null)
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

        var tenant = GetMasterTenant(context);

        var userName = $"{(catalogueRoleName ?? "plain").ToLowerInvariant()}-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

        var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(Password));
        securityUser.ExemptFromPasswordChange();
        context.SecurityUsers.Add(securityUser);
        user.AssignSecurityUser(securityUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Plain membership in the master tenant (no per-tenant role any more).
        tenant.AddUser(user.Id);
        await context.SaveChangesAsync();

        if (catalogueRoleName is not null)
        {
            var role = context.Roles.AsEnumerable().FirstOrDefault(r => r.Name.Value == catalogueRoleName);
            if (role is null)
            {
                role = new Role(new RoleName(catalogueRoleName));
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            context.UserRoles.Add(new UserRole(user.Id, role.Id));
            await context.SaveChangesAsync();
        }

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

    private async Task<HttpClient> LoginInOtherTenantAsync(string? catalogueRoleName = null)
    {
        await using var context = app.CreateDbContext();
        var passwordHasher = app.PasswordHasher;

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport")
            ?? IdentificationType.Create("Passport");
        if (context.IdentificationTypes.Local.All(i => i.Id != identificationType.Id))
            context.IdentificationTypes.Add(identificationType);

        var otherTenant = Tenant.Create($"Other-{Guid.NewGuid():N}");
        context.Tenants.Add(otherTenant);

        var userName = $"{(catalogueRoleName ?? "plain").ToLowerInvariant()}-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Test", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, otherTenant.Id);

        var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(Password));
        securityUser.ExemptFromPasswordChange();
        context.SecurityUsers.Add(securityUser);
        user.AssignSecurityUser(securityUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        otherTenant.AddUser(user.Id);
        await context.SaveChangesAsync();

        if (catalogueRoleName is not null)
        {
            var role = context.Roles.AsEnumerable().FirstOrDefault(r => r.Name.Value == catalogueRoleName);
            if (role is null)
            {
                role = new Role(new RoleName(catalogueRoleName));
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            context.UserRoles.Add(new UserRole(user.Id, role.Id));
            await context.SaveChangesAsync();
        }

        var client = app.CreateClient();
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = userName,
            ["password"] = Password
        });
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/auth/login", form)).StatusCode);

        return client;
    }

    [Fact]
    public async Task Manager_CanListUsers_ButCannotListRoles()
    {
        // /api/users is under ManagerOrAdmin → Manager passes.
        var mgrClient = await LoginInMasterTenantAsync(CatalogueManager);
        var usersResponse = await mgrClient.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        // /api/roles is under AdminOnly → Manager is denied.
        var rolesResponse = await mgrClient.GetAsync("/api/roles");
        Assert.Equal(HttpStatusCode.Forbidden, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task NonMasterAdministrator_CanReadUsers_ButCannotCreateTenant()
    {
        // /api/users under ManagerOrAdmin: "Administrator" catalogue role grants the
        // "Administrator" claim, which satisfies the policy.
        var adminClient = await LoginInOtherTenantAsync(CatalogueAdministrator);
        var usersResponse = await adminClient.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        // MasterAdminOnly on /api/tenants: the user is not in the master tenant, so
        // they cannot create tenants.
        var createResponse = await adminClient.PostAsJsonAsync("/api/tenants", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task MasterAdmin_CanCreateTenant()
    {
        var client = await LoginInMasterTenantAsync(CatalogueAdministrator);

        var response = await client.PostAsJsonAsync("/api/tenants", new
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            OwnerId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task MasterAdmin_WithoutCatalogueRole_StillResolvesAsAdmin()
    {
        // Defensive: a master-tenant user who somehow lost their catalogue role but still
        // has the membership row should NOT be promoted to master admin. Master admin
        // requires BOTH the catalogue role AND User.TenantId == master tenant.
        var client = await LoginInMasterTenantAsync();
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NonMasterAdministrator_CannotAccessOidcCatalogue()
    {
        var client = await LoginInOtherTenantAsync(CatalogueAdministrator);

        var appsResponse = await client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.Forbidden, appsResponse.StatusCode);

        var scopesResponse = await client.GetAsync("/api/scopes");
        Assert.Equal(HttpStatusCode.Forbidden, scopesResponse.StatusCode);
    }
}
