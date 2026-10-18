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
    /// Seeds the two example applications the repository ships with. Both are the same SPA; they
    /// differ only in how consent is handled, so the difference can be observed side by side:
    /// <list type="bullet">
    /// <item><c>implicit</c> (the default) hands out the authorization code as soon as the user is
    /// signed in - what an SPA wants, because a consent screen would break silent renew.</item>
    /// <item><c>explicit</c> detours through <c>/Account/Consent</c> on the first authorization, so
    /// the user approves which scopes the client may use and a permanent authorization is recorded
    /// for the next ones. This is the case to reach for when a resource must be brought under the
    /// identity server's control rather than trusted by default.</item>
    /// </list>
    /// Idempotent: an application with the same ClientId is reconciled instead of duplicated.
    /// </summary>
    private static async Task SeedOpenIddictApplicationsAsync(
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var clientId = configuration["SEED_APP_CLIENT_ID"] ?? DefaultSpaClientId;
        var displayName = configuration["SEED_APP_DISPLAY_NAME"]
            ?? configuration["Branding:DisplayName"]
            ?? "OroIdentity Admin SPA";

        // The SPA builds its redirect URI from window.location.origin, so what has to be
        // registered is "<origin>/auth/callback" for whatever origin the dev server is actually
        // published on - not a hardcoded pair that silently drifts from the AppHost.
        var origin = ResolveSpaOrigin(configuration);
        var redirectUris = new List<string>
        {
            configuration["SEED_APP_REDIRECT_URI"] ?? $"{origin}{SpaCallbackPath}"
        };
        var postLogoutUris = new List<string>
        {
            configuration["SEED_APP_POST_LOGOUT_URI"] ?? origin
        };

        foreach (var extra in SplitCsv(configuration["SEED_APP_EXTRA_ORIGINS"]))
        {
            if (!redirectUris.Contains($"{extra}{SpaCallbackPath}"))
                redirectUris.Add($"{extra}{SpaCallbackPath}");
            if (!postLogoutUris.Contains(extra))
                postLogoutUris.Add(extra);
        }

        await EnsureApplicationAsync(
            applicationManager,
            BuildSpaDescriptor(clientId, displayName, ConsentTypes.Implicit, redirectUris, postLogoutUris),
            cancellationToken);

        // The explicit-consent twin. It gets its own origin because it runs as a second dev server
        // so both examples can be open at once; sharing the implicit one would make each client
        // accept the other's callback.
        var explicitOrigin = ResolveExplicitSpaOrigin(configuration);
        await EnsureApplicationAsync(
            applicationManager,
            BuildSpaDescriptor(
                configuration["SEED_APP_EXPLICIT_CLIENT_ID"] ?? DefaultExplicitSpaClientId,
                configuration["SEED_APP_EXPLICIT_DISPLAY_NAME"]
                    ?? $"{displayName} (explicit consent)",
                ConsentTypes.Explicit,
                [configuration["SEED_APP_EXPLICIT_REDIRECT_URI"] ?? $"{explicitOrigin}{SpaCallbackPath}"],
                [configuration["SEED_APP_EXPLICIT_POST_LOGOUT_URI"] ?? explicitOrigin]),
            cancellationToken);
    }

    /// <summary>Path the Angular SPA listens on for the OIDC redirect (<c>redirectUrl</c>).</summary>
    private const string SpaCallbackPath = "/auth/callback";

    /// <summary>
    /// Client id the <c>oroidentity-admin</c> Angular example asks for. Kept in sync with the
    /// AppHost's <c>ClientId</c> parameter through <c>SEED_APP_CLIENT_ID</c>.
    /// </summary>
    private const string DefaultSpaClientId = "OroIdentityServer.Admin";

    /// <summary>
    /// Client id of the explicit-consent example, i.e. the same SPA run against
    /// <c>OroIdentityServer.Admin.Explicit</c> so both consent behaviours can be compared.
    /// </summary>
    private const string DefaultExplicitSpaClientId = "OroIdentityServer.Admin.Explicit";

    /// <summary>
    /// The second dev server's origin. Defaults to the conventional <c>4201</c> so the example works
    /// even when the AppHost is not driving it (tests, a plain <c>pnpm start:explicit</c>).
    /// </summary>
    private static string ResolveExplicitSpaOrigin(IConfiguration configuration)
    {
        var configured = configuration["SEED_APP_EXPLICIT_ORIGIN"];
        return string.IsNullOrWhiteSpace(configured) ? "http://localhost:4201" : configured.TrimEnd('/');
    }

    private static IEnumerable<string> SplitCsv(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Where the SPA is served from. <c>IDENTITY_ADMIN_HTTP</c> is what the AppHost injects and
    /// what the admin UI itself advertises; the AppHost's own dev-server port is accepted as a
    /// fallback so a mismatch there cannot lock the seeded client out of its own callback.
    /// </summary>
    private static string ResolveSpaOrigin(IConfiguration configuration)
    {
        var configured = configuration["SEED_APP_ORIGIN"]
            ?? configuration["IDENTITY_ADMIN_HTTP"]
            ?? configuration["IDENTITY_ADMIN_HTTPS"];

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        return "http://localhost:4200";
    }

    /// <summary>
    /// A public, PKCE-only SPA client. Two details are not optional and were the reason the
    /// example app could not complete a login: <c>rst:code</c> (without the matching
    /// <c>rst:*</c> permission OpenIddict rejects <c>response_type=code</c> outright), and
    /// <c>ClientType = Public</c> (a confidential client makes the token endpoint demand a
    /// secret, which a browser SPA cannot have).
    /// <paramref name="consentType"/> is what tells the two examples apart: <c>implicit</c> returns
    /// the code straight away, <c>explicit</c> routes the user through <c>/Account/Consent</c>.
    /// </summary>
    private static OpenIddictApplicationDescriptor BuildSpaDescriptor(
        string clientId,
        string displayName,
        string consentType,
        IEnumerable<string> redirectUris,
        IEnumerable<string> postLogoutUris)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = displayName,
            ConsentType = consentType,
            ApplicationType = ApplicationTypes.Web,
            ClientType = ClientTypes.Public
        };

        descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.Endpoints.EndSession);
        descriptor.Permissions.Add(Permissions.Endpoints.Introspection);
        descriptor.Permissions.Add(Permissions.Endpoints.Revocation);
        descriptor.Permissions.Add("ept:userinfo");

        descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
        descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);

        descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
        descriptor.Permissions.Add(Permissions.ResponseTypes.IdToken);
        descriptor.Permissions.Add(Permissions.ResponseTypes.IdToken + " " + Permissions.ResponseTypes.Token);

        descriptor.Permissions.Add(Permissions.Scopes.Email);
        descriptor.Permissions.Add("scp:offline_access");
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add(Permissions.Scopes.Profile);
        descriptor.Permissions.Add(Permissions.Scopes.Roles);

        descriptor.Permissions.Add("scp:permissions");
        descriptor.Permissions.Add("scp:tenant_id");

        descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);

        foreach (var uri in redirectUris) descriptor.RedirectUris.Add(new Uri(uri));
        foreach (var uri in postLogoutUris) descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

        return descriptor;
    }

    /// <summary>
    /// Makes sure the bootstrap admin user always ends up with everything required by the
    /// post-multi-tenant authorization checks: a TenantUser membership in the master tenant
    /// (so <c>GetTenantsByUserId</c> can resolve the user's home tenant) and the
    /// Administrator catalogue role, which is what <c>IsMasterAdminQuery</c> looks for in
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
            .FirstOrDefault(u => u.NormalizedUserName == seedAdmin.UserName.ToUpperInvariant());
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

        // 2. Home tenant must be the master tenant. IsMasterAdminQuery requires
        //    User.TenantId == masterTenant.Id, so we sync it here for pre-existing rows
        //    via a direct UPDATE to avoid going through User.UpdateDetails (which would
        //    require a non-null IdentificationTypeId that older seed rows might not have).
        if (seedUser.TenantId is null || seedUser.TenantId.Value != masterTenant.Id.Value)
        {
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE \"Users\" SET \"TenantId\" = {0} WHERE \"Id\" = {1}",
                masterTenant.Id.Value, seedUser.Id.Value);
        }

        // 3. Administrator catalogue role. This is what flips IsMasterAdminQuery to true
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

    /// <summary>
    /// Creates the client, or reconciles an existing one so a config change actually reaches the
    /// database. Reconciling the full descriptor (not just the permissions) is what makes
    /// <c>SEED_APP_REDIRECT_URI</c> / <c>rst:code</c> edits take effect without deleting the row.
    /// A masked secret is never written back: the manager's populate step is skipped for it.
    /// </summary>
    private static async Task EnsureApplicationAsync(
        IOpenIddictApplicationManager applicationManager,
        OpenIddictApplicationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(descriptor.ClientId, cancellationToken);
        if (application is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
            return;
        }

        var existing = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(existing, application, cancellationToken);

        // A masked secret ("sk-****xyz") in the request must never overwrite the real one, or the
        // client silently stops authenticating.
        var keepSecret = string.IsNullOrEmpty(descriptor.ClientSecret) || IsMaskedSecret(descriptor.ClientSecret)
            ? existing.ClientSecret
            : descriptor.ClientSecret;

        existing.ClientId = descriptor.ClientId;
        existing.ClientSecret = keepSecret;
        existing.DisplayName = descriptor.DisplayName;
        existing.ClientType = descriptor.ClientType;
        existing.ApplicationType = descriptor.ApplicationType;
        existing.ConsentType = descriptor.ConsentType;

        CopyAll(descriptor.Permissions, existing.Permissions);
        CopyAll(descriptor.Requirements, existing.Requirements);

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

        await applicationManager.UpdateAsync(application, existing, cancellationToken);
    }

    /// <summary>Secrets are shown as "sk-****xyz" once created; that is not a usable credential.</summary>
    private static bool IsMaskedSecret(string secret) => secret.Contains("****", StringComparison.Ordinal);

    private static void CopyAll(ISet<string> source, ISet<string> destination)
    {
        destination.Clear();
        foreach (var value in source)
        {
            destination.Add(value);
        }
    }

    private static async Task EnsureHierarchyRolesAsync(OroIdentityAppContext context, CancellationToken cancellationToken)
    {
        var roleDefinitions = new[]
        {
            (Name: "Administrator", Level: 90, Parent: (string?)null)
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

