// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        OroIdentityAppContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IOpenIddictApplicationManager? applicationManager = null,
        CancellationToken cancellationToken = default)
    {
        Guid userCreateId = Guid.CreateVersion7();
        string tenantName = configuration["SEED_TENANT_NAME"];

        if (!context.IdentificationTypes.Any())
        {
            context.IdentificationTypes.Add(
                new IdentificationType(new("CC")));
        }

        if (!context.Tenants.Any())
        {
            context.Tenants.Add(new Tenant(new(tenantName)));
        }

        await context.SaveChangesAsync(cancellationToken);

        SeedUser  seedAdmin = ResolveSeedAdmin(configuration);
        string adminRoleName = configuration["SEED_ADMIN_ROLE"] ?? DefaultAdminRoleName;
        bool adminMustChangePassword = configuration.GetValue<bool?>("SEED_ADMIN_FORCE_PASSWORD_CHANGE") ?? false;

        if (!context.Users.Any())
        {

            SecurityUser securityUser = SecurityUser.Create(await passwordHasher.HashPassword(seedAdmin.PasswordHash));
            securityUser.ExemptFromPasswordChange();

            context.SecurityUsers.Add(securityUser);
            await context.SaveChangesAsync(cancellationToken);

            var newUser = new User(
                new UserId(Guid.CreateVersion7()),
                seedAdmin.Name,
                "", // middleName
                seedAdmin.LastName,
                seedAdmin.UserName,
                seedAdmin.Email,
                seedAdmin.Identification,
                context.IdentificationTypes.First().Id,
                context.Tenants.First().Id
            );
            newUser.AssignSecurityUser(securityUser);
            context.Users.Add(newUser);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.Permissions.Any())
        {
            Permission permission = Permission.Create("System", "Full access to all resources and actions.", "*", "*", true);
            context.Permissions.Add(permission);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Ensure the admin role exists. The original `if (!context.Roles.Any())` check skipped
        // this branch when ANY role already existed in the table, leaving pre-existing
        // databases without the `adminRoleName` role.
        if (!context.Roles.IgnoreQueryFilters().Any(r => r.Name == new RoleName(adminRoleName)))
        {
            var newRole = new Role(new(adminRoleName));
            await context.Permissions.ForEachAsync(x => newRole.AddPermission(x), cancellationToken);
            context.Roles.Add(newRole);

            await context.SaveChangesAsync(cancellationToken);
        }

        await EnsureHierarchyRolesAsync(context, cancellationToken);

        // Idempotently guarantee the seed admin can navigate the console. The original
        // UserRoles / TenantUsers sections only ran when those tables were completely
        // empty, so a database that already had a single role assignment (e.g. from a
        // previous run) never received the Administrator UserRole, and after MasterAdminOnly
        // was added the admin lost access entirely.
        await EnsureSeedAdminCanNavigateAsync(context, seedAdmin, adminRoleName, tenantName, cancellationToken);

        // Seed OpenIddict applications with introspection permission for logout detection.
        if (applicationManager is not null)
        {
            await SeedOpenIddictApplicationsAsync(applicationManager, configuration, cancellationToken);
        }
    }

    /// <summary>
    /// Seeds a default web application with the permissions required for remote logout detection
    /// via token introspection. The application is idempotent: if a client with the same ClientId
    /// already exists, its permissions are updated.
    /// </summary>
    private static async Task SeedOpenIddictApplicationsAsync(
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var defaultClientId = configuration["SEED_APP_CLIENT_ID"] ?? "oroidentity-admin-spa";
        var defaultDisplayName = configuration["SEED_APP_DISPLAY_NAME"] ?? "OroIdentity Admin SPA";
        var defaultRedirectUri = configuration["SEED_APP_REDIRECT_URI"] ?? "https://localhost:5001/authentication/login-callback";
        var defaultPostLogoutUri = configuration["SEED_APP_POST_LOGOUT_URI"] ?? "https://localhost:5001/authentication/logout-callback";

        var existing = await applicationManager.FindByClientIdAsync(defaultClientId, cancellationToken);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(descriptor, existing, cancellationToken);

            var permissionsUpdated = false;
            if (!descriptor.Permissions.Contains("ept:introspection"))
            {
                descriptor.Permissions.Add("ept:introspection");
                permissionsUpdated = true;
            }
            if (!descriptor.Permissions.Contains("ept:revocation"))
            {
                descriptor.Permissions.Add("ept:revocation");
                permissionsUpdated = true;
            }
            if (permissionsUpdated)
            {
                await applicationManager.UpdateAsync(existing, descriptor, cancellationToken);
            }
            return;
        }

        var appDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = defaultClientId,
            DisplayName = defaultDisplayName,
            ConsentType = ConsentTypes.Implicit,
            ApplicationType = ApplicationTypes.Web
        };

        appDescriptor.Permissions.Add("ept:authorization");
        appDescriptor.Permissions.Add("ept:token");
        appDescriptor.Permissions.Add("ept:end_session");
        appDescriptor.Permissions.Add("ept:introspection");
        appDescriptor.Permissions.Add("ept:revocation");
        appDescriptor.Permissions.Add("ept:userinfo");

        appDescriptor.Permissions.Add("gt:authorization_code");
        appDescriptor.Permissions.Add("gt:refresh_token");

        appDescriptor.Permissions.Add("scp:openid");
        appDescriptor.Permissions.Add("scp:profile");
        appDescriptor.Permissions.Add("scp:email");
        appDescriptor.Permissions.Add("scp:roles");

        appDescriptor.Requirements.Add("ft:pkce");

        appDescriptor.RedirectUris.Add(new Uri(defaultRedirectUri));
        appDescriptor.PostLogoutRedirectUris.Add(new Uri(defaultPostLogoutUri));

        await applicationManager.CreateAsync(appDescriptor, cancellationToken);
    }

    /// <summary>
    /// Makes sure the bootstrap admin user always ends up with everything required by the
    /// post-multi-tenant authorization checks: a TenantUser membership in the master tenant
    /// (so <c>GetTenantsByUserId</c> can resolve the user's home tenant) and the
    /// Administrator catalogue role, which is what <c>IsMasterAdminAsync</c> looks for in
    /// combination with the master tenant id to grant the <c>is_master_admin</c> claim.
    /// </summary>
    private static async Task EnsureSeedAdminCanNavigateAsync(
        OroIdentityAppContext context,
        SeedUser seedAdmin,
        string adminRoleName,
        string masterTenantName,
        CancellationToken cancellationToken)
    {
        var seedUser = context.Users.IgnoreQueryFilters()
            .FirstOrDefault(u => u.NormalizedUserName.Equals(seedAdmin.UserName, StringComparison.InvariantCultureIgnoreCase));
        if (seedUser is null)
        {
            return;
        }

        var masterTenant = await context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Name == new TenantName(masterTenantName), cancellationToken)
            ?? await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(cancellationToken);
        if (masterTenant is null)
        {
            return;
        }

        // 1. TenantUser membership in the master tenant. The row only stores the membership
        //    now (no per-tenant Role column); the user's permissions come from the catalogue
        //    UserRole checked below. Home tenant is recorded on User.TenantId.
        var adminTenantUser = await context.TenantUsers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(tu => tu.UserId == seedUser.Id && tu.TenantId == masterTenant.Id, cancellationToken);
        if (adminTenantUser is null)
        {
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"TenantUsers\" (\"Id\", \"TenantId\", \"UserId\", \"IsActive\", \"JoinedAtUtc\") VALUES ({0}, {1}, {2}, {3}, {4})",
                Guid.CreateVersion7(), masterTenant.Id.Value, seedUser.Id.Value, true, DateTime.UtcNow);
        }

        // 2. Home tenant must be the master tenant. IsMasterAdminAsync requires
        //    User.TenantId == masterTenant.Id, so we sync it here for pre-existing rows
        //    via a direct UPDATE to avoid going through User.UpdateDetails (which would
        //    require a non-null IdentificationTypeId that older seed rows might not have).
        if (seedUser.TenantId is null || seedUser.TenantId.Value != masterTenant.Id.Value)
        {
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE \"Users\" SET \"TenantId\" = {0} WHERE \"Id\" = {1}",
                masterTenant.Id.Value, seedUser.Id.Value);
        }

        // 3. Administrator catalogue role. This is what flips IsMasterAdminAsync to true
        //    and what causes BuildPrincipalAsync to issue the "Admin" and "Administrator"
        //    claim pair. Idempotent: pre-existing rows are detected and skipped.
        var adminRole = context.Roles.IgnoreQueryFilters()
            .FirstOrDefault(r => r.Name == new RoleName(adminRoleName));
        if (adminRole is not null)
        {
            var hasAdminUserRole = await context.UserRoles.IgnoreQueryFilters()
                .AnyAsync(ur => ur.UserId == seedUser.Id && ur.RoleId == adminRole.Id, cancellationToken);
            if (!hasAdminUserRole)
            {
                context.UserRoles.Add(new UserRole(seedUser.Id, adminRole.Id));
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static async Task EnsureApplicationAsync(
        IOpenIddictApplicationManager applicationManager,
        OpenIddictApplicationDescriptor descriptor)
    {
        var application = await applicationManager.FindByClientIdAsync(descriptor.ClientId);
        if (application is null)
        {
            await applicationManager.CreateAsync(descriptor);
            return;
        }

        var existing = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(existing, application);
        existing.ClientId = descriptor.ClientId;
        existing.DisplayName = descriptor.DisplayName;
        existing.ClientSecret = descriptor.ClientSecret;
        existing.ClientType = descriptor.ClientType;
        existing.ApplicationType = descriptor.ApplicationType;
        existing.ConsentType = descriptor.ConsentType;

        existing.Permissions.Clear();
        foreach (var permission in descriptor.Permissions)
        {
            existing.Permissions.Add(permission);
        }

        existing.Requirements.Clear();
        foreach (var requirement in descriptor.Requirements)
        {
            existing.Requirements.Add(requirement);
        }

        existing.RedirectUris.Clear();
        foreach (var redirectUri in descriptor.RedirectUris)
        {
            existing.RedirectUris.Add(redirectUri);
        }

        existing.PostLogoutRedirectUris.Clear();
        foreach (var redirectUri in descriptor.PostLogoutRedirectUris)
        {
            existing.PostLogoutRedirectUris.Add(redirectUri);
        }

        await applicationManager.UpdateAsync(application, existing);
    }

    private static async Task EnsureHierarchyRolesAsync(OroIdentityAppContext context, CancellationToken cancellationToken)
    {
        var roleDefinitions = new[]
        {
            (Name: "Administrator", Level: 90, Parent: (string?)null),
            (Name: "Manager", Level: 70, Parent: "Administrator"),
            (Name: "User", Level: 10, Parent: "Manager"),
        };

        var existingRoles = await context.Roles.IgnoreQueryFilters().ToListAsync(cancellationToken);
        var byName = existingRoles.ToDictionary(r => r.Name.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var (Name, Level, Parent) in roleDefinitions)
        {
            if (!byName.TryGetValue(Name, out var role))
            {
                RoleId? parentId = null;
                if (Parent != null && byName.TryGetValue(Parent, out var parent))
                    parentId = parent.Id;
                var newRole = new Role(new RoleName(Name), Level, parentId);
                context.Roles.Add(newRole);
                byName[Name] = newRole;
            }
            else
            {
                if (role.Level != Level)
                {
                    role.SetLevel(Level);
                }
                if (Parent != null && byName.TryGetValue(Parent, out var parent))
                {
                    if (role.ParentRoleId?.Value != parent.Id.Value)
                        role.SetParentRole(parent.Id);
                }
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminRoleName = "Administrator";

    /// <summary>
    /// Resolves the universal bootstrap admin user from environment variables, falling back to the
    /// values baked into the seed file. The resolved user is the one granted the Administrator role
    /// and (by default) exempted from the forced first-login password change.
    /// </summary>
    private static SeedUser ResolveSeedAdmin(IConfiguration configuration)
    {
        var adminUserName = configuration["SEED_ADMIN_USERNAME"] ?? DefaultAdminUserName;

        var seedUser = new SeedUser
        {
            UserName = adminUserName,
            Name = configuration["SEED_ADMIN_NAME"] ?? string.Empty,
            LastName = configuration["SEED_ADMIN_LASTNAME"] ?? string.Empty,
            Email = configuration["SEED_ADMIN_EMAIL"] ?? "admin@example.com",
            Identification = configuration["SEED_ADMIN_IDENTIFICATION"] ?? "000000001",
            PasswordHash = configuration["SEED_ADMIN_PASSWORD"] ?? "Admin@123456"
        };

        return seedUser;
    }
}

public class SeedData
{
    public string IdentificationType { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public List<SeedUser> Users { get; set; } = [];
    public List<SeedRole> Roles { get; set; } = [];
}

public class SeedUser
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class SeedRole
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; } = 10;
    public string? ParentRole { get; set; }
}

