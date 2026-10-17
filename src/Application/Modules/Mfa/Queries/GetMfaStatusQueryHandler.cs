// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

using OroIdentityServer.Core.Modules.Tenants.Repositories;
using OroIdentityServer.Core.Modules.Users.Repositories;

public sealed class GetMfaStatusQueryHandler(
    ILogger<GetMfaStatusQueryHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    IUserRepository userRepository,
    ITenantRepository tenantRepository) : IQueryHandler<GetMfaStatusQuery, MfaStatusResponse>
{
    public async Task<MfaStatusResponse> HandleAsync(GetMfaStatusQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetMfaStatusQuery for user {UserId}", query.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(query.UserId), cancellationToken);
            var unusedRecoveryCodes = credential is null
                ? 0
                : (await recoveryCodeRepository.GetUnusedByUserIdAsync(new(query.UserId), cancellationToken)).Count();

            return new MfaStatusResponse
            {
                IsEnabled = credential?.IsEnabled ?? false,
                HasPendingEnrollment = credential is not null && !credential.IsEnabled,
                TenantRequiresMfa = await IsTenantRequiringMfaAsync(query.UserId, cancellationToken),
                UnusedRecoveryCodes = unusedRecoveryCodes,
                ConfirmedAtUtc = credential?.ConfirmedAtUtc,
                LastVerifiedAtUtc = credential?.LastVerifiedAtUtc
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading MFA status for user {UserId}", query.UserId);
            throw;
        }
    }

    /// <summary>
    /// Tenant policy: when it is on, a user without a confirmed credential is pushed through
    /// enrolment at login instead of being locked out.
    /// </summary>
    private async Task<bool> IsTenantRequiringMfaAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(new(userId), cancellationToken);
        if (user?.TenantId is null) return false;

        var tenant = await tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        return tenant?.RequireMfa ?? false;
    }
}