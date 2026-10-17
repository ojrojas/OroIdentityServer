// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class MfaRecoveryCodeRepository(
    ILogger<MfaRecoveryCodeRepository> logger,
    IRepository<MfaRecoveryCode> repository) : IMfaRecoveryCodeRepository
{
    public async Task AddAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering AddAsync for MFA recovery code of user {UserId}", code.UserId.Value);
        await repository.AddAsync(code, cancellationToken);
        logger.LogInformation("Exiting AddAsync for MFA recovery code of user {UserId}", code.UserId.Value);
    }

    public async Task UpdateAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering UpdateAsync for MFA recovery code {CodeId}", code.Id.Value);
        await repository.UpdateAsync(code, cancellationToken);
        logger.LogInformation("Exiting UpdateAsync for MFA recovery code {CodeId}", code.Id.Value);
    }

    public async Task DeleteAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering DeleteAsync for MFA recovery code {CodeId}", code.Id.Value);
        await repository.DeleteAsync(code, cancellationToken);
        logger.LogInformation("Exiting DeleteAsync for MFA recovery code {CodeId}", code.Id.Value);
    }

    public async Task<IEnumerable<MfaRecoveryCode>> GetUnusedByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering GetUnusedByUserIdAsync");
        var codes = await repository.FindAsync(c => c.UserId == userId && !c.IsUsed, cancellationToken);
        logger.LogInformation("Exiting GetUnusedByUserIdAsync with {Count} codes", codes.Count());
        return codes;
    }

    public async Task<IEnumerable<MfaRecoveryCode>> GetByCredentialIdAsync(MfaCredentialId credentialId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering GetByCredentialIdAsync");
        var codes = await repository.FindAsync(c => c.MfaCredentialId == credentialId, cancellationToken);
        logger.LogInformation("Exiting GetByCredentialIdAsync");
        return codes;
    }
}