// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

using OroIdentityServer.Core.Modules.Tenants.Repositories;
using OroIdentityServer.Core.Modules.Users.Repositories;

public sealed class IsMfaRequiredQueryHandler(
    ILogger<IsMfaRequiredQueryHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IUserRepository userRepository,
    ITenantRepository tenantRepository) : IQueryHandler<IsMfaRequiredQuery, MfaRequirementResponse>
{
    public async Task<MfaRequirementResponse> HandleAsync(IsMfaRequiredQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling IsMfaRequiredQuery for user {UserId}", query.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(query.UserId), cancellationToken);
            if (credential?.IsEnabled == true)
                return new MfaRequirementResponse(IsRequired: true, RequiresEnrollment: false);

            if (!await IsTenantPolicyEnabledAsync(query.UserId, cancellationToken))
                return new MfaRequirementResponse(IsRequired: false, RequiresEnrollment: false);

            logger.LogInformation("Tenant policy requires MFA for user {UserId} without an enrolled credential", query.UserId);
            return new MfaRequirementResponse(IsRequired: true, RequiresEnrollment: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving the MFA requirement for user {UserId}", query.UserId);
            throw;
        }
    }

    private async Task<bool> IsTenantPolicyEnabledAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(new(userId), cancellationToken);
        if (user?.TenantId is null) return false;

        var tenant = await tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        return tenant?.RequireMfa ?? false;
    }
}