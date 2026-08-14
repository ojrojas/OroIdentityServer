// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Roles.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Core.Services;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Server.Authentication;

namespace OroIdentityServer.Infraestructure.UnitTests;

/// <summary>
/// Coverage for the catalogue-only role model. TenantUser rows now only record
/// membership (which users belong to which tenant); the actual role a user has is
/// derived from the catalogue UserRole + the user's home tenant:
///   * Master admin (User.TenantId == SEED_TENANT_NAME AND catalogue role "Administrator")
///     gets the "Admin" + "Administrator" claim pair plus is_master_admin.
///   * Non-master "Administrator" catalogue members get only "Administrator" (no master
///     claim, can't open OIDC pages that are gated by MasterAdminOnly).
///   * "Manager" catalogue members get "Manager".
///   * "User" or no catalogue role → "Member".
/// </summary>
public class AssignAdministratorRoleReproTests
{
    private const string MasterTenantName = "OroMasterTenant";
    private const string Password = "Abc123456#";
    private const string CatalogueAdministrator = "Administrator";
    private const string CatalogueManager = "Manager";

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SEED_TENANT_NAME"] = MasterTenantName,
            })
            .Build();

    private static (AdminPasswordSignInService signIn, OroIdentityAppContext context) CreateSignInService()
    {
        var context = TestDbContextFactory.CreateSqlite();

        var userRepo = new UserRepository(
            NullLogger<UserRepository>.Instance,
            new Repository<User>(NullLogger<Repository<User>>.Instance, context),
            Substitute.For<ISecurityUserRepository>());
        var securityUserRepo = new SecurityUserRepository(
            NullLogger<SecurityUserRepository>.Instance,
            new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context));
        var tenantRepo = new TenantRepository(
            NullLogger<TenantRepository>.Instance,
            new Repository<Tenant>(NullLogger<Repository<Tenant>>.Instance, context));
        var hasher = new PasswordHasher();

        var signIn = new AdminPasswordSignInService(
            NullLogger<AdminPasswordSignInService>.Instance,
            userRepo,
            securityUserRepo,
            tenantRepo,
            hasher,
            BuildConfiguration());

        return (signIn, context);
    }

    private static IdentificationType EnsurePassport(OroIdentityAppContext context)
    {
        var existing = context.IdentificationTypes
            .AsEnumerable()
            .FirstOrDefault(i => i.Name.Value == "Passport");
        if (existing is not null) return existing;

        var created = IdentificationType.Create("Passport");
        context.IdentificationTypes.Add(created);
        return created;
    }

    [Fact]
    public async Task SeedAdmin_WithAdministratorCatalogueRoleInMasterTenant_GetsAdminAndAdministratorAndIsMasterAdmin()
    {
        // Mirrors the seed: admin user in OroMasterTenant with the "Administrator"
        // catalogue role. The principal must carry BOTH "Admin" and "Administrator"
        // role claims so the user passes every Razor page (the canonical mapping
        // Admin,Administrator,Manager / Admin,Administrator) plus the is_master_admin
        // claim for the MasterAdminOnly policy.
        var (signIn, context) = CreateSignInService();
        var userId = await SeedMasterTenantAdminWithCatalogueRoleAsync(context, CatalogueAdministrator);

        var userName = context.Users.Single(u => u.Id == new UserId(userId)).UserName!;
        var principal = await signIn.SignInAsync(userName, Password, CancellationToken.None);

        Assert.NotNull(principal);
        Assert.True(principal.Identity?.IsAuthenticated);

        var roleClaims = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Administrator", roleClaims);
        Assert.DoesNotContain("Member", roleClaims);
        Assert.True(principal.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true"));

        // Password-change refresh must keep the same identity.
        var refreshed = await signIn.RefreshPrincipalAsync(userId, CancellationToken.None);
        Assert.NotNull(refreshed);
        var refreshedRoles = refreshed!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Admin", refreshedRoles);
        Assert.Contains("Administrator", refreshedRoles);
        Assert.True(refreshed.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true"));
    }

    [Fact]
    public async Task UserWithAdministratorCatalogueRole_OutsideMasterTenant_GetsAdministratorButNotMasterAdmin()
    {
        // A user that carries the "Administrator" catalogue role and lives in a non-master
        // tenant is an app admin: they pass the [Authorize(Roles="Admin,Administrator")]
        // checks on Razor pages, but MasterAdminOnly keeps the OIDC console out of reach.
        var (signIn, context) = CreateSignInService();

        var identificationType = EnsurePassport(context);
        var masterTenant = new Tenant(new TenantName(MasterTenantName));
        context.Tenants.Add(masterTenant);
        var otherTenant = new Tenant(new TenantName($"Tenant-{Guid.NewGuid():N}"));
        context.Tenants.Add(otherTenant);
        await context.SaveChangesAsync();

        var userName = $"app-admin-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "AppAdmin", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, otherTenant.Id);
        user.AssignSecurityUser(SecurityUser.Create(await new PasswordHasher().HashPassword(Password)));
        context.Users.Add(user);

        // Membership in the other tenant (no per-tenant role any more).
        var membership = otherTenant.AddUser(user.Id);
        context.TenantUsers.Add(membership);

        // Administrator catalogue role.
        var role = new Role(new RoleName(CatalogueAdministrator));
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var principal = await signIn.SignInAsync(userName, Password, CancellationToken.None);

        Assert.NotNull(principal);
        var roles = principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Administrator", roles);
        Assert.DoesNotContain("Admin", roles);
        Assert.False(principal.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true"));
    }

    [Fact]
    public async Task UserWithManagerCatalogueRole_GetsManagerClaim()
    {
        // A "Manager" catalogue member gets the Manager role claim (passes the
        // Admin,Administrator,Manager Razor pages but not the Admin,Administrator-only
        // OIDC pages, and not MasterAdminOnly).
        var (signIn, context) = CreateSignInService();

        var identificationType = EnsurePassport(context);
        var masterTenant = new Tenant(new TenantName(MasterTenantName));
        context.Tenants.Add(masterTenant);
        var tenant = new Tenant(new TenantName($"Tenant-{Guid.NewGuid():N}"));
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userName = $"mgr-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Mgr", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        user.AssignSecurityUser(SecurityUser.Create(await new PasswordHasher().HashPassword(Password)));
        context.Users.Add(user);

        var membership = tenant.AddUser(user.Id);
        context.TenantUsers.Add(membership);

        var role = new Role(new RoleName(CatalogueManager));
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        var principal = await signIn.SignInAsync(userName, Password, CancellationToken.None);

        Assert.NotNull(principal);
        var roles = principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Manager", roles);
        Assert.DoesNotContain("Administrator", roles);
        Assert.DoesNotContain("Admin", roles);
        Assert.False(principal.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true"));
    }

    [Fact]
    public async Task UserWithNoCatalogueRole_FallsBackToMember()
    {
        // No catalogue assignment, plain membership → "Member" claim, no master admin.
        // The dashboard's NotAuthorized handler will redirect to /access-denied.
        var (signIn, context) = CreateSignInService();

        var identificationType = EnsurePassport(context);
        var tenant = new Tenant(new TenantName($"Tenant-{Guid.NewGuid():N}"));
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var userName = $"plain-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Plain", "", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);
        user.AssignSecurityUser(SecurityUser.Create(await new PasswordHasher().HashPassword(Password)));
        context.Users.Add(user);

        var membership = tenant.AddUser(user.Id);
        context.TenantUsers.Add(membership);
        await context.SaveChangesAsync();

        var principal = await signIn.SignInAsync(userName, Password, CancellationToken.None);

        Assert.NotNull(principal);
        var roles = principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Member", roles);
        Assert.DoesNotContain("Administrator", roles);
        Assert.DoesNotContain("Admin", roles);
        Assert.False(principal.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true"));
    }

    private static async Task<Guid> SeedMasterTenantAdminWithCatalogueRoleAsync(
        OroIdentityAppContext context,
        string catalogueRoleName)
    {
        var identificationType = EnsurePassport(context);

        var masterTenant = new Tenant(new TenantName(MasterTenantName));
        context.Tenants.Add(masterTenant);

        var userName = $"admin-{Guid.NewGuid():N}";
        var user = User.Create(
            userName, $"{userName}@example.com", "Admin", "Master", "User",
            Guid.NewGuid().ToString("N"), identificationType.Id, masterTenant.Id);
        user.AssignSecurityUser(SecurityUser.Create(await new PasswordHasher().HashPassword(Password)));
        context.Users.Add(user);

        // Membership in the master tenant (the row is the only thing TenantUser carries
        // now; the role comes from the catalogue).
        var membership = masterTenant.AddUser(user.Id);
        context.TenantUsers.Add(membership);

        var role = new Role(new RoleName(catalogueRoleName));
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole(user.Id, role.Id));
        await context.SaveChangesAsync();

        return user.Id.Value;
    }
}
