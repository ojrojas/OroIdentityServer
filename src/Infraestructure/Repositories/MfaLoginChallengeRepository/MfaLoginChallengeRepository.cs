// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

using OroIdentityServer.Core.Modules.Mfa.Enums;

public class MfaLoginChallengeRepository(
    ILogger<MfaLoginChallengeRepository> logger,
    IRepository<MfaLoginChallenge> repository) : IMfaLoginChallengeRepository
{
    public async Task AddAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering AddAsync for MFA challenge of user {UserId}", challenge.UserId.Value);
        await repository.AddAsync(challenge, cancellationToken);
        logger.LogInformation("Exiting AddAsync for MFA challenge of user {UserId}", challenge.UserId.Value);
    }

    public async Task UpdateAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering UpdateAsync for MFA challenge {ChallengeId}", challenge.Id.Value);
        await repository.UpdateAsync(challenge, cancellationToken);
        logger.LogInformation("Exiting UpdateAsync for MFA challenge {ChallengeId}", challenge.Id.Value);
    }

    public async Task DeleteAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering DeleteAsync for MFA challenge {ChallengeId}", challenge.Id.Value);
        await repository.DeleteAsync(challenge, cancellationToken);
        logger.LogInformation("Exiting DeleteAsync for MFA challenge {ChallengeId}", challenge.Id.Value);
    }

    public async Task<MfaLoginChallenge?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering GetByTokenAsync");
        var challenge = await repository.FindSingleAsync(c => c.Token == token, cancellationToken);
        logger.LogInformation("Exiting GetByTokenAsync");
        return challenge;
    }

    public async Task<IEnumerable<MfaLoginChallenge>> GetOpenByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Entering GetOpenByUserIdAsync");
        var challenges = await repository.FindAsync(
            c => c.UserId == userId
                 && (c.Status == MfaChallengeStatus.Pending || c.Status == MfaChallengeStatus.Failed),
            cancellationToken);
        logger.LogInformation("Exiting GetOpenByUserIdAsync");
        return challenges;
    }
}