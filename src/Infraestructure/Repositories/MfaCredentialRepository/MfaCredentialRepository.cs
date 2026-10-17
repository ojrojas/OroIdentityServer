// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class MfaCredentialRepository(
    ILogger<MfaCredentialRepository> logger,
    IRepository<MfaCredential> repository) : IMfaCredentialRepository
{
    public async Task AddAsync(MfaCredential credential, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering AddAsync for MFA credential of user {UserId}", credential.UserId.Value);
        await repository.AddAsync(credential, cancellationToken);
        logger.LogInformation("Exiting AddAsync for MFA credential of user {UserId}", credential.UserId.Value);
    }

    public async Task UpdateAsync(MfaCredential credential, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering UpdateAsync for MFA credential {CredentialId}", credential.Id.Value);
        await repository.UpdateAsync(credential, cancellationToken);
        logger.LogInformation("Exiting UpdateAsync for MFA credential {CredentialId}", credential.Id.Value);
    }

    public async Task DeleteAsync(MfaCredential credential, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering DeleteAsync for MFA credential {CredentialId}", credential.Id.Value);
        await repository.DeleteAsync(credential, cancellationToken);
        logger.LogInformation("Exiting DeleteAsync for MFA credential {CredentialId}", credential.Id.Value);
    }

    public async Task<MfaCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering GetByUserIdAsync");
        var credential = await repository.FindSingleAsync(c => c.UserId == userId, cancellationToken);
        logger.LogInformation("Exiting GetByUserIdAsync");
        return credential;
    }
}