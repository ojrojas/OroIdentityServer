// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Queries;

/// <summary>
/// Evaluates master-admin membership from the catalogue roles and the home tenant. The role
/// names mirror the seeded catalogue (<c>DatabaseSeeder</c>); this handler is the single place
/// that maps them to the "master admin" answer, so the cookie principal and the OIDC tokens
/// cannot disagree about it.
/// </summary>
public sealed class IsMasterAdminQueryHandler(
    ILogger<IsMasterAdminQueryHandler> logger,
    IUserRepository userRepository,
    ITenantRepository tenantRepository) : IQueryHandler<IsMasterAdminQuery, IsMasterAdminResponse>
{
    private const string AdministratorRoleName = "Administrator";
    private const string AdminRoleName = "Admin";

    public async Task<IsMasterAdminResponse> HandleAsync(IsMasterAdminQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(new(query.UserId), cancellationToken);
        if (user is null)
        {
            return new IsMasterAdminResponse { Data = false };
        }

        var masterTenant = await tenantRepository.FindSingleAsync(
            t => t.Name == new TenantName(query.MasterTenantName), cancellationToken);
        if (masterTenant is null)
        {
            logger.LogWarning("Master tenant '{MasterTenantName}' was not found.", query.MasterTenantName);
            return new IsMasterAdminResponse { Data = false };
        }

        if (user.TenantId is null || user.TenantId.Value != masterTenant.Id.Value)
        {
            return new IsMasterAdminResponse { Data = false };
        }

        var isMasterAdmin = user.Roles.Any(ur => ur.Role is not null
            && (string.Equals(ur.Role.Name.Value, AdministratorRoleName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ur.Role.Name.Value, AdminRoleName, StringComparison.OrdinalIgnoreCase)));

        return new IsMasterAdminResponse { Data = isMasterAdmin };
    }
}
