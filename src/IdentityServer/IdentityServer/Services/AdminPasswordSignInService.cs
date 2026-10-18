// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace IdentityServer.Services;

using System.Security.Claims;
using System.Text.Json;
using BuildingBlocks.CQRS.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OroIdentityServer.Application.Modules.Hierarchy.Queries;
using OroIdentityServer.Application.Modules.Permissions.Queries;
using OroIdentityServer.Application.Modules.Users.Queries;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Server.Authentication;
using OroIdentityServer.Server.Authorization;
using OroIdentityServer.Shared.Authorization;

public interface IAdminPasswordSignInService
{
    Task<ClaimsPrincipal?> BuildPrincipalForSignedInUserAsync(Guid userId, CancellationToken ct);
    Task<ClaimsPrincipal?> RefreshPrincipalAsync(Guid userId, CancellationToken ct);
    Task<ClaimsPrincipal?> SignInAsync(string loginIdentifier, string password, CancellationToken ct);
}

/// <summary>
/// Builds the admin principal from the password login and the second-factor steps. It is a
/// Server-layer orchestrator: every fact it needs (identity, password validity, master-admin
/// membership, hierarchy claims, permission names) arrives through
/// <see cref="IQueryDispatcher"/>, never through a repository or the DbContext. That keeps the
/// layering one-directional (Server → Application → Core/Infraestructure) and means the cookie
/// principal and the OIDC tokens are assembled from the same queries.
/// </summary>
public sealed class AdminPasswordSignInService : IAdminPasswordSignInService
{
    private readonly ILogger<AdminPasswordSignInService> _logger;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly string _masterTenantName;

    public AdminPasswordSignInService(
        ILogger<AdminPasswordSignInService> logger,
        IQueryDispatcher queryDispatcher,
        IConfiguration configuration)
    {
        _logger = logger;
        _queryDispatcher = queryDispatcher;
        _masterTenantName = configuration["SEED_TENANT_NAME"] ?? DefaultMasterTenantName;
    }

    public const string MustChangePasswordClaimType = "must_change_password";

    private const string DefaultMasterTenantName = "OroMasterTenant";

    public async Task<ClaimsPrincipal?> SignInAsync(string loginIdentifier, string password, CancellationToken ct)
    {
        try
        {
            var byLogin = await _queryDispatcher.SendAsync(
                new GetUserByLoginIdentifierQuery(loginIdentifier), ct);
            if (byLogin.Data is null)
            {
                _logger.LogWarning("Login failed: user not found for {LoginIdentifier}", loginIdentifier);
                return null;
            }

            var passwordCheck = await _queryDispatcher.SendAsync(
                new ValidateUserPasswordQuery(loginIdentifier, password), ct);
            if (passwordCheck.Data is not true)
            {
                _logger.LogWarning("Login failed: invalid password for {LoginIdentifier}", loginIdentifier);
                return null;
            }

            var byId = await _queryDispatcher.SendAsync(new GetUserByIdQuery(byLogin.Data.Id), ct);
            if (byId.Data is null)
            {
                _logger.LogWarning("Login failed: user not found for {LoginIdentifier}", loginIdentifier);
                return null;
            }

            return await BuildPrincipalAsync(
                byId.Data,
                byId.Data.SecurityUser?.MustChangePassword == true,
                loginIdentifier,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login failed for {LoginIdentifier}", loginIdentifier);
            return null;
        }
    }

    /// <summary>
    /// Rebuilds the admin cookie principal for a user that just changed their password, so the
    /// "must change password" claim (and any redirect enforced by it) is cleared without requiring
    /// them to log in again.
    /// </summary>
    public async Task<ClaimsPrincipal?> RefreshPrincipalAsync(Guid userId, CancellationToken ct)
    {
        var response = await _queryDispatcher.SendAsync(new GetUserByIdQuery(userId), ct);
        if (response.Data is null) return null;

        var user = response.Data;
        return await BuildPrincipalAsync(
            user,
            mustChangePassword: false,
            user.UserName ?? user.Email ?? string.Empty,
            ct);
    }

    /// <summary>
    /// Rebuilds the principal for a user that just cleared the second factor. Same principal as a
    /// password sign-in, including the must-change-password claim, so the two entry points cannot
    /// end up with different entitlements.
    /// </summary>
    public async Task<ClaimsPrincipal?> BuildPrincipalForSignedInUserAsync(Guid userId, CancellationToken ct)
    {
        var response = await _queryDispatcher.SendAsync(new GetUserByIdQuery(userId), ct);
        if (response.Data is null) return null;

        var user = response.Data;
        if (user.SecurityUserId is null || user.SecurityUser is null) return null;

        return await BuildPrincipalAsync(
            user,
            user.SecurityUser.MustChangePassword,
            user.UserName ?? user.Email ?? string.Empty,
            ct);
    }

    private async Task<ClaimsPrincipal> BuildPrincipalAsync(
        User user, bool mustChangePassword, string loginIdentifier, CancellationToken ct)
    {
        // The user's "role" lives in the catalogue (UserRole -> Role.Name), not in any
        // per-tenant membership column. GetUserByIdQuery includes Roles.Role, so the navigation
        // is populated without a second round trip.
        var catalogueRoleNames = user.Roles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var masterAdmin = await _queryDispatcher.SendAsync(
            new IsMasterAdminQuery(user.Id.Value, _masterTenantName), ct);

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
        if (masterAdmin.Data is true)
        {
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Admin));
            claims.Add(new Claim(ClaimTypes.Role, TenantRole.Administrator));
            claims.Add(new Claim(AuthorizationClaimTypes.IsMasterAdmin, "true"));
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

        // Hierarchy claims. A failure here must not fail the login: the user authenticated
        // fine, so warn and continue with a principal that simply lacks hierarchy detail.
        if (user.TenantId is not null)
        {
            try
            {
                var hierarchy = await _queryDispatcher.SendAsync(
                    new GetUserHierarchyClaimsQuery(user.TenantId.Value, user.Id.Value), ct);
                if (hierarchy.Data is not null)
                {
                    claims.Add(new Claim(HierarchyClaimTypes.HierarchyLevel, hierarchy.Data.HierarchyLevel.ToString()));
                    claims.Add(new Claim(HierarchyClaimTypes.DirectSuperiorIds, JsonSerializer.Serialize(hierarchy.Data.DirectSuperiorIds)));
                    if (hierarchy.Data.PrimarySuperiorId is not null)
                        claims.Add(new Claim(HierarchyClaimTypes.PrimarySuperiorId, hierarchy.Data.PrimarySuperiorId));
                    claims.Add(new Claim(HierarchyClaimTypes.RelationshipTypes, JsonSerializer.Serialize(hierarchy.Data.RelationshipTypes)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add hierarchy claims for user {UserId}", user.Id.Value);
            }
        }

        // Domain user permissions, one "permission" claim per granted permission name.
        try
        {
            var permissions = await _queryDispatcher.SendAsync(
                new GetPermissionNamesByUserIdQuery(user.Id.Value), ct);
            foreach (var name in permissions.Data ?? [])
                claims.Add(new Claim(AuthorizationClaimTypes.Permission, name));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add permission claims for user {UserId}", user.Id.Value);
        }

        var identity = new ClaimsIdentity(claims, CookieAuthHandlerSetup.AdminScheme);
        return new ClaimsPrincipal(identity);
    }
}
