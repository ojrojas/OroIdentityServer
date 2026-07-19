// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;

namespace OroIdentityServer.Server.Authentication;

public sealed class AdminPasswordSignInService(
    ILogger<AdminPasswordSignInService> logger,
    IUserRepository userRepository,
    ISecurityUserRepository securityUserRepository,
    IPasswordHasher passwordHasher,
    IConfiguration configuration)
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

        return BuildPrincipal(user, securityUser.MustChangePassword, loginIdentifier);
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

        return BuildPrincipal(user, mustChangePassword: false, user.UserName ?? user.Email ?? string.Empty);
    }

    private ClaimsPrincipal BuildPrincipal(User user, bool mustChangePassword, string loginIdentifier)
    {
        var defaultRole = configuration["Admin:DefaultRole"] ?? "Administrator";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? loginIdentifier),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, defaultRole),
            new("tenant_id", user.TenantId!.Value.ToString())
        };

        if (mustChangePassword)
            claims.Add(new Claim(MustChangePasswordClaimType, "true"));

        var identity = new ClaimsIdentity(claims, CookieAuthHandlerSetup.AdminScheme);
        return new ClaimsPrincipal(identity);
    }
}
