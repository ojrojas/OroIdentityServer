// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        OroIdentityAppContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
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

        if (!context.Roles.Any())
        {

            var newRole = new Role(new(adminRoleName));
            await context.Permissions.ForEachAsync(x => newRole.AddPermission(x), cancellationToken);
            context.Roles.Add(newRole);

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
            var tenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(cancellationToken);

            if (tenant is not null)
            {
                foreach (var user in context.Users.ToList())
                {
                    var role = user.UserName!.Equals(seedAdmin.UserName, StringComparison.OrdinalIgnoreCase)
                        ? TenantRole.Admin
                        : TenantRole.Member;

                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO \"TenantUsers\" (\"Id\", \"TenantId\", \"UserId\", \"Role\", \"IsActive\", \"JoinedAtUtc\") VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                        Guid.CreateVersion7(), tenant.Id.Value, user.Id.Value, role, true, DateTime.UtcNow);
                }
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
}

