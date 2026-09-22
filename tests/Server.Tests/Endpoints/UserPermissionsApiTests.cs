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
/// End-to-end coverage for direct user permissions:
///   * an Administrator can grant a permission directly to a user;
///   * the effective-permissions endpoint reflects it;
///   * unknown permission ids are rejected.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class UserPermissionsApiTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";
    private const string MasterTenantName = "OroMasterTenant";
    private const string CatalogueAdministrator = "Administrator";

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

    private static (Guid UserId, Guid PermissionId) SeedTargetUserAndPermission(OroIdentityAppContext context)
    {
        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport")
            ?? IdentificationType.Create("Passport");
        if (context.IdentificationTypes.Local.All(i => i.Id != identificationType.Id))
            context.IdentificationTypes.Add(identificationType);

        var tenant = context.Tenants.AsEnumerable().First(t => t.Name.Value == MasterTenantName);

        var target = User.Create(
            $"target-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Target", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        context.Users.Add(target);

        var permission = Permission.Create("oropos", "Read sales", "read", "sales", false);
        context.Permissions.Add(permission);

        context.SaveChanges();
        return (target.Id.Value, permission.Id.Value);
    }

    [Fact]
    public async Task Administrator_CanGrantDirectPermission_AndEffectiveEndpointReflectsIt()
    {
        Guid userId;
        Guid permissionId;
        await using (var context = app.CreateDbContext())
        {
            (userId, permissionId) = SeedTargetUserAndPermission(context);
        }

        var client = await LoginAsync(CatalogueAdministrator);

        var response = await client.PutAsJsonAsync(
            $"/api/users/{userId}/permissions",
            new { permissionIds = new[] { permissionId } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using (var context = app.CreateDbContext())
        {
            var assigned = context.UserPermissions
                .AsEnumerable()
                .Where(up => up.UserId.Value == userId)
                .ToList();
            Assert.Contains(assigned, up => up.PermissionId.Value == permissionId);
        }

        var effective = await client.GetFromJsonAsync<EffectivePermissionsResponse>(
            $"/api/users/{userId}/effective-permissions");
        Assert.NotNull(effective);
        Assert.Contains("oropos.sales.read", effective!.Data ?? []);
    }

    [Fact]
    public async Task UnknownPermission_IsRejected()
    {
        Guid userId;
        await using (var context = app.CreateDbContext())
        {
            (userId, _) = SeedTargetUserAndPermission(context);
        }

        var client = await LoginAsync(CatalogueAdministrator);

        var response = await client.PutAsJsonAsync(
            $"/api/users/{userId}/permissions",
            new { permissionIds = new[] { Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record EffectivePermissionsResponse(int StatusCode, List<string>? Data);
}
