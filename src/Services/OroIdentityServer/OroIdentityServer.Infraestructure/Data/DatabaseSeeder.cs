// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        OroIdentityAppContext context,
        IOpenIddictApplicationManager applicationManager,
        string jsonFilePath,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        Guid userCreateId = Guid.CreateVersion7();
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Seed data file not found: {jsonFilePath}");
        }

        var jsonData = await File.ReadAllTextAsync(jsonFilePath);
        var seedData = JsonSerializer.Deserialize<SeedData>(jsonData) ?? throw new InvalidOperationException("Failed to deserialize seed data.");
        if (!context.IdentificationTypes.Any())
        {
            context.IdentificationTypes.Add(
                new IdentificationType(new(seedData.IdentificationType)));
        }

        await context.SaveChangesAsync();

        if (!context.Users.Any())
        {
            foreach (var user in seedData.Users)
            {
                var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(user.PasswordHash));
                context.SecurityUsers.Add(securityUser);
                await context.SaveChangesAsync();

                var newUser = new User(
                    new UserId(Guid.CreateVersion7()),
                    user.Name,
                    "", // middleName
                    user.LastName,
                    user.UserName,
                    user.Email,
                    user.Identification,
                    context.IdentificationTypes.First().Id
                );
                newUser.AssignSecurityUser(securityUser);
                context.Users.Add(newUser);
            }
            await context.SaveChangesAsync();
        }

        if (!context.Roles.Any())
        {
            foreach (var role in seedData.Roles)
            {
                var newRole = new Role(role.Name);
                foreach (var roleClaim in role.RoleClaims)
                {
                    newRole.AddClaim(new RoleClaim(new RoleClaimType(roleClaim.ClaimType), new RoleClaimValue(roleClaim.ClaimValue)));
                }
                context.Roles.Add(newRole);
            }
            await context.SaveChangesAsync();
        }

        if (!context.UserRoles.Any())
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.Name != null && r.Name.Value == "Administrator");
            var userRole = context.Roles.FirstOrDefault(r => r.Name != null && r.Name.Value == "User");

            if (adminRole != null && userRole != null)
            {
                var pepe = context.Users.FirstOrDefault(u => u.Email == "pepe@example.com");
                var maria = context.Users.FirstOrDefault(u => u.Email == "maria@example.com");

                if (pepe != null)
                {
                    context.UserRoles.Add(new UserRole(pepe.Id, adminRole.Id));
                }

                if (maria != null)
                {
                    context.UserRoles.Add(new UserRole(maria.Id, userRole.Id));
                }
            }
        }

        // Register OpenIddict application
        if (await applicationManager.FindByClientIdAsync("OroIdentityServer.Web") == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "OroIdentityServer.Web",
                DisplayName = "OroIdentityServer Web Application",
                ClientSecret = "a2344152-e928-49e7-bb3c-ee54acc96c8c",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.ResponseTypes.Code,
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = { new Uri($"{configuration["IdentityWeb:Url"]}/signin-oidc") },
                PostLogoutRedirectUris = { new Uri($"{configuration["IdentityWeb:Url"]}/signout-callback-oidc") }
            });
        }

        await context.SaveChangesAsync();
    }
}

public class SeedData
{
    public string IdentificationType { get; set; } = string.Empty;
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
    public List<SeedRoleClaim> RoleClaims { get; set; } = [];
}

public class SeedRoleClaim
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}