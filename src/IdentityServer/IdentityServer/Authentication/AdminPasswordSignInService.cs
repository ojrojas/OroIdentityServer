// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Repositories;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Shared.Authorization;

namespace OroIdentityServer.Server.Authentication;

/// <summary>
/// The canonical names of the roles seeded in the catalogue (see <c>DatabaseSeeder</c> and
/// <c>seedData.json</c>). Sign-in reads these to translate a user's catalogue memberships
/// into the claim names that the [Authorize] policies and Razor page attributes check.
/// </summary>
public static class CatalogueRole
{
    public const string Administrator = nameof(Administrator);
    public const string Admin = nameof(Admin);
    public const string Manager = nameof(Manager);
    public const string User = nameof(User);
}

public sealed class AdminPasswordSignInService(
    ILogger<AdminPasswordSignInService> logger,
    IUserRepository userRepository,
    ISecurityUserRepository securityUserRepository,
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher,
    IConfiguration configuration)
{
    public const string MustChangePasswordClaimType = "must_change_password";

    /// <summary>
    /// Kept as an alias for <see cref="AuthorizationClaimTypes.IsMasterAdmin"/> so
    /// existing call sites in this assembly and the Server.Tests project still
    /// resolve. New code should prefer the shared constant.
    /// </summary>
    public const string IsMasterAdminClaimType = AuthorizationClaimTypes.IsMasterAdmin;

    private const string DefaultMasterTenantName = "OroMasterTenant";

    public async Task<ClaimsPrincipal?> SignInAsync(string loginIdentifier, string password, CancellationToken ct)
    {
        User? user;
        try
        {
            user = await userRepository.GetUserByLoginIdentifierAsync(loginIdentifier, ct);
        }
        catch
        {
            logger.LogWarning("Login failed: user not found for {LoginIdentifier}", loginIdentifier);
            return null;
        }

        if (user?.SecurityUserId is null)
        {
            logger.LogWarning("Login failed: security user missing for {LoginIdentifier}", loginIdentifier);
            return null;
        }

        var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId!.Value, ct);
        if (securityUser?.PasswordHash is null)
        {
            logger.LogWarning("Login failed: security user missing for {LoginIdentifier}", loginIdentifier);
            return null;
        }

        if (!await passwordHasher.VerifyPassword(password, securityUser.PasswordHash))
        {
            logger.LogWarning("Login failed: invalid password for {LoginIdentifier}", loginIdentifier);
            return null;
        }

        return await BuildPrincipalAsync(user, securityUser.MustChangePassword, loginIdentifier, ct);
    }

    /// <summary>
    /// Rebuilds the admin cookie principal for a user that just changed their password, so the
    /// "must change password" claim (and any redirect enforced by it) is cleared without requiring
    /// them to log in again.
    /// </summary>
    public async Task<ClaimsPrincipal?> RefreshPrincipalAsync(Guid userId, CancellationToken ct)
    {
        var user = await userRepository.GetUserByIdAsync(new(userId), ct);
        if (user is null) return null;

        return await BuildPrincipalAsync(user, mustChangePassword: false, user.UserName ?? user.Email ?? string.Empty, ct);
    }

    private async Task<ClaimsPrincipal> BuildPrincipalAsync(User user, bool mustChangePassword, string loginIdentifier, CancellationToken ct)
    {
        // The user's "role" lives in the catalogue (UserRole -> Role.Name), not in any
        // per-tenant membership column. The spec that loaded this User included
        // "Roles.Role", so the navigation is populated.
        var catalogueRoleNames = user.Roles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var isMasterAdmin = await IsMasterAdminAsync(user, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? loginIdentifier),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(AuthorizationClaimTypes.TenantId, user.TenantId?.Value.ToString() ?? string.Empty)
        };

        // The master admin of the SEED_TENANT_NAME gets BOTH "Admin" and "Administrator" role
        // claims so they pass every Razor page that gates on either. Other users get exactly
        // the claim that matches their highest catalogue role, falling back to "Member" for
        // anyone who only has the read-only "User" role or none at all.
        if (isMasterAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Admin));
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Administrator));
            claims.Add(new Claim(IsMasterAdminClaimType, "true"));
        }
        else if (catalogueRoleNames.Contains(CatalogueRole.Administrator))
        {
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Administrator));
        }
        else if (catalogueRoleNames.Contains(CatalogueRole.Manager))
        {
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Manager));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Member));
        }

        if (mustChangePassword)
            claims.Add(new Claim(MustChangePasswordClaimType, "true"));

        var identity = new ClaimsIdentity(claims, CookieAuthHandlerSetup.AdminScheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// True when the user is the master admin: a user with the Administrator catalogue role
    /// whose home tenant (<c>User.TenantId</c>) is the <c>SEED_TENANT_NAME</c>. The principal
    /// issued by <see cref="BuildPrincipalAsync"/> keys off this same answer, so the two
    /// stay in sync. Admins of other tenants exist in the IdP as app admins but must not
    /// navigate or manage the IdP web console.
    /// </summary>
    public async Task<bool> IsMasterAdminAsync(User user, CancellationToken ct)
    {
        var masterTenantName = configuration["SEED_TENANT_NAME"] ?? DefaultMasterTenantName;
        var masterTenant = await tenantRepository.FindSingleAsync(t => t.Name == new TenantName(masterTenantName), ct);
        if (masterTenant is null) return false;

        if (user.TenantId is null || user.TenantId.Value != masterTenant.Id.Value) return false;

        return user.Roles.Any(ur => ur.Role is not null
            && string.Equals(ur.Role.Name.Value, CatalogueRole.Administrator, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ur.Role.Name.Value, CatalogueRole.Admin, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The tenants a user is allowed to navigate. Master admins see every active tenant; everyone
    /// else sees only the tenants they have a TenantUser membership in (the read-only "User"
    /// catalogue role is enough to be a member, but only the master admin sees the full
    /// catalogue of tenants).
    /// </summary>
    public async Task<IReadOnlyList<Tenant>> GetAccessibleTenantsAsync(User user, CancellationToken ct)
    {
        if (await IsMasterAdminAsync(user, ct))
        {
            return (await tenantRepository.GetAllAsync(ct)).ToList();
        }

        return (await tenantRepository.GetByUserIdAsync(user.Id, ct)).ToList();
    }
}
