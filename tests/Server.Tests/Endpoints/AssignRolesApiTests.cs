// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;
using IdentityServer.Client.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

public sealed class AssignRolesApiTests(IdentityServerWebApplicationFactory factory)
    : IClassFixture<IdentityServerWebApplicationFactory>
{
    private const string Password = "Abc123456#";

    private async Task<HttpClient> LoginAsAdminAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (identificationType is null)
        {
            identificationType = IdentificationType.Create("Passport");
            context.IdentificationTypes.Add(identificationType);
        }

        var tenant = Tenant.Create($"Tenant-Admin-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

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

        tenant.AddUser(user.Id, OroIdentityServer.Core.Modules.Tenants.ValueObjects.TenantRole.Admin);
        await context.SaveChangesAsync();

        var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = userName,
            ["password"] = Password
        });

        var response = await client.PostAsync("/auth/login", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    private async Task<(Guid userId, Guid roleId, Guid roleId2)> SeedTargetUserAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (identificationType is null)
        {
            identificationType = IdentificationType.Create("Passport");
            context.IdentificationTypes.Add(identificationType);
        }

        var tenant = Tenant.Create($"Tenant-Target-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var user = User.Create(
            $"target-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Target", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

        var securityUser = SecurityUser.Create("hash");
        user.AssignSecurityUser(securityUser);
        context.SecurityUsers.Add(securityUser);
        context.Users.Add(user);

        var role1 = new Role(new RoleName($"Role1-{Guid.NewGuid():N}"));
        var role2 = new Role(new RoleName($"Role2-{Guid.NewGuid():N}"));
        context.Roles.AddRange(role1, role2);

        var userRole = new UserRole(user.Id, role1.Id);
        user.AddRole(userRole);
        context.UserRoles.Add(userRole);

        await context.SaveChangesAsync();

        return (user.Id.Value, role1.Id.Value, role2.Id.Value);
    }

    [Fact]
    public async Task AssignRoles_Add_Then_Remove_ShouldSucceed()
    {
        var client = await LoginAsAdminAsync();
        var (userId, roleId, roleId2) = await SeedTargetUserAsync();

        var addResponse = await client.PutAsJsonAsync(
            $"/api/users/{userId}/roles",
            new AssignRolesRequest([roleId, roleId2]));
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var removeResponse = await client.PutAsJsonAsync(
            $"/api/users/{userId}/roles",
            new AssignRolesRequest([roleId]));
        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldSucceed()
    {
        var client = await LoginAsAdminAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();

        var identificationType = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (identificationType is null)
        {
            identificationType = IdentificationType.Create("Passport");
            context.IdentificationTypes.Add(identificationType);
        }

        var tenant = Tenant.Create($"Tenant-Edit-{Guid.NewGuid():N}");
        context.Tenants.Add(tenant);

        var user = User.Create(
            $"edit-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", "Edit", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/users/{user.Id.Value}",
            new UpdateUserRequest(
                "EditedName", "", "EditedLast", $"edited-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com",
                "Abc123456#", Guid.NewGuid().ToString("N"), identificationType.Id.Value, tenant.Id.Value));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteRole_SoftDeletes_And_DisappearsFromList()
    {
        var client = await LoginAsAdminAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();

        var role = new Role(new RoleName($"SoftDelete-{Guid.NewGuid():N}"));
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var deleteResponse = await client.DeleteAsync($"/api/roles/{role.Id.Value}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await client.GetFromJsonAsync<ApiResponse<List<RoleModel>>>("/api/roles");
        Assert.NotNull(listResponse);
        Assert.DoesNotContain(listResponse.Data ?? [], r => r.Id == role.Id.Value);
    }
}
