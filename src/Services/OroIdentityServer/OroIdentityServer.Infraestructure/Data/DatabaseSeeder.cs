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
        // if (!context.IdentificationTypes.Any())
        // {
        //     context.IdentificationTypes.Add(
        //         new IdentificationType(new(seedData.IdentificationType))
        //         {
        //             CreatedBy = userCreateId
        //         });
        // }

        // await context.SaveChangesAsync();

        // if (!context.Users.Any())
        // {
        //     foreach (var user in seedData.Users)
        //         context.Users.Add(new User
        //         {
        //             Name = user.Name,
        //             LastName = user.LastName,
        //             UserName = user.UserName,
        //             Email = user.Email,
        //             Identification = user.Identification,
        //             IdentificationTypeId = context.IdentificationTypes.FirstOrDefault()!.Id,
        //             SecurityUser = new SecurityUser
        //             {
        //                 PasswordHash = await passwordHasher.HashPassword(user.PasswordHash),
        //                 SecurityStamp = Guid.NewGuid().ToString(),
        //                 ConcurrencyStamp = Guid.NewGuid()
        //             },
        //             CreatedBy = userCreateId
        //         });
        // }

        // if (!context.Roles.Any())
        // {
        //     foreach (var role in seedData.Roles)
        //     {
        //         var newRole = new Role(new RoleName(role.Name))
        //         {
        //             ConcurrencyStamp = Guid.CreateVersion7(),
        //             CreatedBy = userCreateId
        //         };

        //         context.Roles.Add(newRole);
        //     }

        //     await context.SaveChangesAsync(); // Save roles first to generate IDs

        //     foreach (var role in seedData.Roles)
        //     {
        //         var dbRole = context.Roles.FirstOrDefault(r => r.RoleName.Value == role.Name);
        //         if (dbRole != null)
        //         {
        //             foreach (var roleClaim in role.RoleClaims)
        //             {
        //                 context.RoleClaims.Add(new RoleClaim
        //                 {
        //                     RoleId = dbRole.Id,
        //                     ClaimType = roleClaim.ClaimType,
        //                     ClaimValue = roleClaim.ClaimValue,
        //                     CreatedBy = userCreateId
        //                 });
        //             }
        //         }
        //     }
        // }

        // if (!context.Set<UserRoles>().Any())
        // {
        //     var adminRole = context.Roles.FirstOrDefault(r => r.RoleName.Value == "Administrator");
        //     var userRole = context.Roles.FirstOrDefault(r => r.RoleName.Value == "User");

        //     if (adminRole != null && userRole != null)
        //     {
        //         var pepe = context.Users.FirstOrDefault(u => u.Email == "pepe@example.com");
        //         var maria = context.Users.FirstOrDefault(u => u.Email == "maria@example.com");

        //         if (pepe != null)
        //         {
        //             context.Set<UserRoles>().Add(new UserRoles
        //             {
        //                 UserId = pepe.Id,
        //                 RoleId = adminRole.Id,
        //                 CreatedBy = userCreateId
        //             });
        //         }

        //         if (maria != null)
        //         {
        //             context.Set<UserRoles>().Add(new UserRoles
        //             {
        //                 UserId = maria.Id,
        //                 RoleId = userRole.Id,
        //                 CreatedBy = userCreateId
        //             });
        //         }
        //     }
        // }

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