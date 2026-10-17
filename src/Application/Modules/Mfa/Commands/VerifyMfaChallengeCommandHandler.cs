// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>
/// The gate between "password accepted" and "session issued". Three things are enforced here:
/// the challenge must be alive, the attempt budget must not be exhausted (a six digit code is
/// only ~10^6 values, so an uncapped retry loop would be a real attack), and a code may not be
/// replayed inside a time step that has already been spent.
/// </summary>
public sealed class VerifyMfaChallengeCommandHandler(
    ILogger<VerifyMfaChallengeCommandHandler> logger,
    IMfaLoginChallengeRepository challengeRepository,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IRecoveryCodeGenerator recoveryCodeGenerator,
    IPasswordHasher passwordHasher) : ICommandHandler<VerifyMfaChallengeCommand, MfaChallengeVerification>
{
    public async Task<MfaChallengeVerification> HandleAsync(VerifyMfaChallengeCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling VerifyMfaChallengeCommand");

        try
        {
            var challenge = await challengeRepository.GetByTokenAsync(command.ChallengeToken, cancellationToken);
            if (challenge is null)
            {
                logger.LogWarning("Rejected MFA submission: unknown challenge token");
                return Failure(Guid.Empty, MfaFailureReason.UnknownChallenge);
            }

            var userId = challenge.UserId.Value;
            var now = DateTime.UtcNow;

            // An enrolment challenge is completed by confirming the first code against the newly
            // issued secret, never by this command.
            if (challenge.Purpose == MfaChallengePurpose.Enroll)
                return Failure(userId, MfaFailureReason.NotEnrolled);

            if (challenge.Status is MfaChallengeStatus.Completed or MfaChallengeStatus.Superseded)
                return Failure(userId, MfaFailureReason.UnknownChallenge);

            if (challenge.Status == MfaChallengeStatus.Locked)
                return Failure(userId, MfaFailureReason.ChallengeLocked);

            if (challenge.IsExpired(now))
            {
                challenge.RegisterFailedAttempt(now);
                await challengeRepository.UpdateAsync(challenge, cancellationToken);
                return Failure(userId, MfaFailureReason.ChallengeExpired);
            }

            var credential = await credentialRepository.GetByUserIdAsync(challenge.UserId, cancellationToken);
            if (credential is null || !credential.IsEnabled)
                return Failure(userId, MfaFailureReason.NotEnrolled);

            var totpVerified = TryVerifyTotp(credential, command.Code, now);

            // Recovery codes are only considered when the entry was not a valid TOTP, so a
            // six-digit TOTP can never be mistaken for a recovery code.
            var usedRecoveryCode = !totpVerified
                && await TryConsumeRecoveryCodeAsync(credential, command.Code, now, cancellationToken);

            if (!totpVerified && !usedRecoveryCode)
                return await RejectAttemptAsync(challenge, userId, now, cancellationToken);

            if (usedRecoveryCode)
                credential.RecordVerifiedUse(totpService.GetTimeStep(now), now);

            challenge.Complete(now);
            await challengeRepository.UpdateAsync(challenge, cancellationToken);
            await credentialRepository.UpdateAsync(credential, cancellationToken);

            logger.LogInformation("MFA challenge {ChallengeId} cleared for user {UserId} (recovery code: {UsedRecoveryCode})",
                challenge.Id.Value, userId, usedRecoveryCode);

            return new MfaChallengeVerification(
                Succeeded: true,
                UserId: userId,
                IsLocked: false,
                UsedRecoveryCode: usedRecoveryCode,
                RequiresEnrollment: false,
                AttemptsRemaining: 0,
                ReturnUrl: challenge.ReturnUrl,
                FailureReason: MfaFailureReason.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying MFA challenge");
            throw;
        }
    }

    /// <summary>
    /// A code whose time step was already consumed counts as invalid: it is a replay.
    /// </summary>
    private bool TryVerifyTotp(MfaCredential credential, string code, DateTime now)
    {
        var secret = secretProtector.Unprotect(credential.EncryptedSecret);
        if (!totpService.TryVerifyCode(secret, code, now, out var timeStep))
            return false;

        if (timeStep <= credential.LastUsedTimeStep)
        {
            logger.LogWarning("Rejected replayed TOTP code for credential {CredentialId}", credential.Id.Value);
            return false;
        }

        credential.RecordVerifiedUse(timeStep, now);
        return true;
    }

    private async Task<bool> TryConsumeRecoveryCodeAsync(
        MfaCredential credential,
        string code,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        var normalized = recoveryCodeGenerator.Normalize(code);
        if (normalized.Length == 0) return false;

        foreach (var candidate in await recoveryCodeRepository.GetUnusedByUserIdAsync(credential.UserId, cancellationToken))
        {
            if (!await passwordHasher.VerifyPassword(normalized, candidate.CodeHash))
                continue;

            candidate.MarkUsed(now);
            await recoveryCodeRepository.UpdateAsync(candidate, cancellationToken);
            return true;
        }

        return false;
    }

    private async Task<MfaChallengeVerification> RejectAttemptAsync(
        MfaLoginChallenge challenge,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var stillUsable = challenge.RegisterFailedAttempt(now);
        await challengeRepository.UpdateAsync(challenge, cancellationToken);

        logger.LogWarning(
            "MFA challenge {ChallengeId} rejected a code for user {UserId} (attempt {Attempts}/{MaxAttempts})",
            challenge.Id.Value, userId, challenge.Attempts, MfaLoginChallenge.MaxAttempts);

        return stillUsable
            ? Failure(userId, MfaFailureReason.InvalidCode, challenge.Attempts)
            : Failure(userId, MfaFailureReason.ChallengeLocked, challenge.Attempts);
    }

    private static MfaChallengeVerification Failure(Guid userId, int reason, int attempts = 0) => new(
        Succeeded: false,
        UserId: userId,
        IsLocked: reason is MfaFailureReason.ChallengeLocked,
        UsedRecoveryCode: false,
        RequiresEnrollment: false,
        AttemptsRemaining: Math.Max(0, MfaLoginChallenge.MaxAttempts - attempts),
        ReturnUrl: null,
        FailureReason: reason);
}