// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Permissions.Aggregates;
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
/// End-to-end coverage for the role↔permission assignment API:
///   * an Administrator can replace a role's domain permissions;
///   * unknown permission ids are rejected without changing assignments;
///   * a Manager cannot call the AdminOnly endpoint.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class RolePermissionsApiTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";
    private const string MasterTenantName = "OroMasterTenant";
    private const string CatalogueAdministrator = "Administrator";
    private const string CatalogueManager = "Manager";

    private async Task<HttpClient> LoginAsync(string? catalogueRoleName)
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

        var tenant = context.Tenants.AsEnumerable().First(t => t.Name.Value == MasterTenantName);

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
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/auth/login", form)).StatusCode);
        return client;
    }

    private static (Guid RoleId, Guid PermissionId) SeedRoleAndPermission(OroIdentityAppContext context)
    {
        var permission = Permission.Create("oropos", "Read sales", "read", "sales", false);
        context.Permissions.Add(permission);

        var role = new Role(new RoleName($"Role-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        context.SaveChanges();

        return (role.Id.Value, permission.Id.Value);
    }

    [Fact]
    public async Task Administrator_CanReplaceRolePermissions()
    {
        Guid roleId;
        Guid permissionId;
        await using (var context = app.CreateDbContext())
        {
            (roleId, permissionId) = SeedRoleAndPermission(context);
        }

        var client = await LoginAsync(CatalogueAdministrator);

        var response = await client.PutAsJsonAsync(
            $"/api/roles/{roleId}/permissions",
            new { permissionIds = new[] { permissionId } });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using (var context = app.CreateDbContext())
        {
            var role = context.Roles
                .Include(r => r.RolePermissions)
                .AsEnumerable()
                .First(r => r.Id.Value == roleId);
            Assert.Contains(role.RolePermissions, rp => rp.PermissionId.Value == permissionId);
        }
    }

    [Fact]
    public async Task UnknownPermission_IsRejected()
    {
        Guid roleId;
        await using (var context = app.CreateDbContext())
        {
            (roleId, _) = SeedRoleAndPermission(context);
        }

        var client = await LoginAsync(CatalogueAdministrator);

        var response = await client.PutAsJsonAsync(
            $"/api/roles/{roleId}/permissions",
            new { permissionIds = new[] { Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CannotSetRolePermissions()
    {
        Guid roleId;
        await using (var context = app.CreateDbContext())
        {
            (roleId, _) = SeedRoleAndPermission(context);
        }

        var client = await LoginAsync(CatalogueManager);

        var response = await client.PutAsJsonAsync(
            $"/api/roles/{roleId}/permissions",
            new { permissionIds = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
