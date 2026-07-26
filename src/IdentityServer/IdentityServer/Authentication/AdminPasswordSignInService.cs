// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Tenants.Repositories;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Server.Authentication;

public sealed class AdminPasswordSignInService(
    ILogger<AdminPasswordSignInService> logger,
    IUserRepository userRepository,
    ISecurityUserRepository securityUserRepository,
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher)
{
    public const string MustChangePasswordClaimType = "must_change_password";

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
        var role = await ResolveTenantRoleAsync(user, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? loginIdentifier),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("tenant_id", user.TenantId!.Value.ToString())
        };

        if (mustChangePassword)
            claims.Add(new Claim(MustChangePasswordClaimType, "true"));

        var identity = new ClaimsIdentity(claims, CookieAuthHandlerSetup.AdminScheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// A user's admin-console role is scoped to their home tenant's TenantUser membership, not a
    /// global claim - defaults to the least-privileged role if no membership row exists yet.
    /// </summary>
    private async Task<string> ResolveTenantRoleAsync(User user, CancellationToken ct)
    {
        var tenants = await tenantRepository.GetByUserIdAsync(user.Id, ct);
        var homeTenant = tenants.FirstOrDefault(t => t.Id == user.TenantId);
        var membership = homeTenant?.TenantUsers.FirstOrDefault(tu => tu.UserId == user.Id);

        return membership?.Role ?? TenantRole.Member;
    }
}
