// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        OroIdentityAppContext context,
        IOpenIddictApplicationManager applicationManager,
        string jsonFilePath,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IOpenIddictScopeManager? scopeManager = null, CancellationToken cancellationToken = default)
    {
        Guid userCreateId = Guid.CreateVersion7();
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Seed data file not found: {jsonFilePath}");
        }

        var jsonData = await File.ReadAllTextAsync(jsonFilePath, cancellationToken);
        var seedData = JsonSerializer.Deserialize<SeedData>(jsonData) ?? throw new InvalidOperationException("Failed to deserialize seed data.");
        if (!context.IdentificationTypes.Any())
        {
            context.IdentificationTypes.Add(
                new IdentificationType(new(seedData.IdentificationType)));
        }

        if (!context.Tenants.Any())
        {
            context.Tenants.Add(new Tenant(new(seedData.Tenant)));
        }

        await context.SaveChangesAsync(cancellationToken);

        var seedAdmin = ResolveSeedAdmin(seedData, configuration);
        var adminRoleName = configuration["SEED_ADMIN_ROLE"] ?? DefaultAdminRoleName;
        var adminMustChangePassword = configuration.GetValue<bool?>("SEED_ADMIN_FORCE_PASSWORD_CHANGE") ?? false;

        if (!context.Users.Any())
        {
            foreach (var user in seedData.Users)
            {
                var isAdmin = ReferenceEquals(user, seedAdmin);

                var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(user.PasswordHash));
                if (isAdmin && !adminMustChangePassword)
                    securityUser.ExemptFromPasswordChange();

                context.SecurityUsers.Add(securityUser);
                await context.SaveChangesAsync(cancellationToken);

                var newUser = new User(
                    new UserId(Guid.CreateVersion7()),
                    user.Name,
                    "", // middleName
                    user.LastName,
                    user.UserName,
                    user.Email,
                    user.Identification,
                    context.IdentificationTypes.First().Id,
                    context.Tenants.First().Id
                );
                newUser.AssignSecurityUser(securityUser);
                context.Users.Add(newUser);
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.Permissions.Any())
        {
            Permission permission = Permission.Create("System", "Full access to all resources and actions.", "*", "*", true);
            context.Permissions.Add(permission);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.Roles.Any())
        {
            foreach (var role in seedData.Roles)
            {
                var newRole = new Role(new(role.Name));
                await context.Permissions.ForEachAsync(x => newRole.AddPermission(x), cancellationToken);
                context.Roles.Add(newRole);
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.UserRoles.Any())
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.Name == new RoleName(adminRoleName));
            var userRole = context.Roles.FirstOrDefault(r => r.Name == new RoleName("User"));

            foreach (var user in context.Users.ToList())
            {
                var roleId = user.UserName!.Equals(seedAdmin.UserName, StringComparison.OrdinalIgnoreCase)
                    ? adminRole?.Id
                    : userRole?.Id;

                if (roleId is not null)
                {
                    context.UserRoles.Add(new UserRole(user.Id, roleId));
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.TenantUsers.Any())
        {
            var tenants = await context.Tenants
                .Include(t => t.TenantUsers)
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            var allUsers = await context.Users.ToListAsync(cancellationToken);

            foreach (var tenant in tenants)
            {
                var usersInTenant = allUsers.Where(u => u.TenantId == tenant.Id);

                foreach (var user in usersInTenant)
                {
                    var role = user.UserName!.Equals(seedAdmin.UserName, StringComparison.OrdinalIgnoreCase)
                        ? TenantRole.Admin
                        : TenantRole.Member;

                    tenant.AddUser(user.Id, role);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        var identityUrls = configuration.GetValue<string>("ASPNETCORE_URLS").Split(";");

        // Register OpenIddict application for server-side web client
        if (await applicationManager.FindByClientIdAsync("OroIdentityServer.Web", cancellationToken) == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "OroIdentityServer.Web",
                DisplayName = "OroIdentityServer Web Application",
                ClientSecret = "a2344152-e928-49e7-bb3c-ee54acc96c8c",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = {
                    new Uri($"{identityUrls.First()}/signin-oidc"),
                    new Uri($"{identityUrls.Last()}/signin-oidc")
                    },
                PostLogoutRedirectUris = {
                    new Uri($"{identityUrls.First()}/signout-callback-oidc"),
                    new Uri($"{identityUrls.Last()}/signout-callback-oidc")
                    }
            }, cancellationToken);
        }

        // Register OpenIddict application for server-side web client
        if (await applicationManager.FindByClientIdAsync("OroIdentityServer.Admin", cancellationToken) == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "OroIdentityServer.Admin",
                DisplayName = "OroIdentityServer Admin Application (Angular SPA Client)",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Native,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                Permissions =
                {
                     OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "accountants-api",
                    OpenIddictConstants.Permissions.ResponseTypes.Code
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = {
                    new Uri($"{configuration["IDENTITY_ADMIN_HTTP"]}") ,
                    new Uri($"{configuration["IDENTITY_ADMIN_HTTP"]}/auth/callback")
                    },
                PostLogoutRedirectUris = { new Uri($"{configuration["IDENTITY_ADMIN_HTTP"].Trim()}") }
            }, cancellationToken);
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

    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminRoleName = "Administrator";

    /// <summary>
    /// Resolves the universal bootstrap admin user from environment variables, falling back to the
    /// values baked into the seed file. The resolved user is the one granted the Administrator role
    /// and (by default) exempted from the forced first-login password change.
    /// </summary>
    private static SeedUser ResolveSeedAdmin(SeedData seedData, IConfiguration configuration)
    {
        var adminUserName = configuration["SEED_ADMIN_USERNAME"] ?? DefaultAdminUserName;

        var candidate = seedData.Users.FirstOrDefault(u => u.UserName.Equals(adminUserName, StringComparison.OrdinalIgnoreCase))
            ?? seedData.Users.FirstOrDefault();

        if (candidate is null)
        {
            candidate = new SeedUser();
            seedData.Users.Insert(0, candidate);
        }

        candidate.UserName = adminUserName;
        candidate.Name = configuration["SEED_ADMIN_NAME"] ?? (string.IsNullOrWhiteSpace(candidate.Name) ? "Admin" : candidate.Name);
        candidate.LastName = configuration["SEED_ADMIN_LASTNAME"] ?? (string.IsNullOrWhiteSpace(candidate.LastName) ? "Administrator" : candidate.LastName);
        candidate.Email = configuration["SEED_ADMIN_EMAIL"] ?? (string.IsNullOrWhiteSpace(candidate.Email) ? "admin@example.com" : candidate.Email);
        candidate.Identification = configuration["SEED_ADMIN_IDENTIFICATION"] ?? (string.IsNullOrWhiteSpace(candidate.Identification) ? "000000001" : candidate.Identification);
        candidate.PasswordHash = configuration["SEED_ADMIN_PASSWORD"] ?? (string.IsNullOrWhiteSpace(candidate.PasswordHash) ? "Admin@123456" : candidate.PasswordHash);

        return candidate;
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
}

