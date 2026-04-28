// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.RoleClaims.Entities;
using OroIdentityServer.Core.Modules.RoleClaims.ValueObjects;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        OroIdentityAppContext context,
        IOpenIddictApplicationManager applicationManager,
        string jsonFilePath,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IOpenIddictScopeManager? scopeManager = null)
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

        if(!context.Tenants.Any())
        {
            context.Tenants.Add(new Tenant(new (seedData.Tenant)));
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
                    context.IdentificationTypes.First().Id,
                    context.Tenants.First().Id
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

        // Register OpenIddict application for server-side web client
        if (await applicationManager.FindByClientIdAsync("OroIdentityServer.Web") == null)
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
                RedirectUris = { new Uri($"{configuration["Identity:Url"]}/signin-oidc") },
                PostLogoutRedirectUris = { new Uri($"{configuration["Identity:Url"]}/signout-callback-oidc") }
            });
        }

        // Register OpenIddict application for OroAccountants Angular SPA
        if (await applicationManager.FindByClientIdAsync("OroAccountants.Web") == null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "OroAccountants.Web",
                DisplayName = "OroAccountants Web Application (Angular SPA)",
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
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            };

            var webOrigins = new[] { "http://localhost:4200", "http://localhost:4300", "http://localhost:5173" };
            var configuredWeb = configuration["ACCOUNTANTS_WEB_HTTP"]?.TrimEnd('/');

            if (!string.IsNullOrEmpty(configuredWeb))
            {
                descriptor.RedirectUris.Add(new Uri($"{configuredWeb}/auth/callback"));
                descriptor.RedirectUris.Add(new Uri($"{configuredWeb}/"));
                descriptor.PostLogoutRedirectUris.Add(new Uri($"{configuredWeb}"));
            }

            foreach (var origin in webOrigins)
            {
                if (string.Equals(origin, configuredWeb, StringComparison.OrdinalIgnoreCase))
                    continue;
                descriptor.RedirectUris.Add(new Uri($"{origin}/auth/callback"));
                descriptor.RedirectUris.Add(new Uri($"{origin}/"));
                descriptor.PostLogoutRedirectUris.Add(new Uri($"{origin}/"));
            }

            await applicationManager.CreateAsync(descriptor);
        }

        // Register OpenIddict application for OroAccountants Core API (server-to-server)
        if (await applicationManager.FindByClientIdAsync("OroAccountants.Api") == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "OroAccountants.Api",
                DisplayName = "OroAccountants Core API",
                ClientSecret = "b3455263-f039-5ae8-cc65-ff65bdd97d9d",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "ai-api",
                }
            });
        }

        // Register OpenIddict application for OroAccountants AI API (server-to-server)
        await EnsureApplicationAsync(
            applicationManager,
            new OpenIddictApplicationDescriptor
            {
                ClientId = "OroAccountants.AI",
                DisplayName = "OroAccountants AI Service API",
                ClientSecret = "c4566374-g140-6bf9-dd76-gg76cee08e0e",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "accountants-api",
                }
            });

        // Register custom scopes for OroAccountants APIs
        if (scopeManager != null)
        {
            if (await scopeManager.FindByNameAsync("accountants-api") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "accountants-api",
                    DisplayName = "OroAccountants Core API Access",
                    Resources = { "OroAccountants.Api" }
                });
            }

            if (await scopeManager.FindByNameAsync("ai-api") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "ai-api",
                    DisplayName = "OroAccountants AI Service API Access",
                    Resources = { "OroAccountants.AI" }
                });
            }
        }

        await context.SaveChangesAsync();
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
}

public class SeedData
{
    public string IdentificationType { get; set; } = string.Empty;
    public string Tenant {get;set;} = string.Empty;
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
