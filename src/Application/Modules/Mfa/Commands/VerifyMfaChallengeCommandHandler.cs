// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Modules.Mfa;
using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.Exceptions;

/// <summary>
/// The gate between "password accepted" and "session issued". Four things are enforced here:
/// the challenge must be alive, the per-challenge attempt budget must not be exhausted (a six
/// digit code is only ~10^6 values, so an uncapped retry loop would be a real attack), the
/// credential must not be serving a time-based lockout, and a code may not be replayed inside a
/// time step that has already been spent.
/// </summary>
public sealed class VerifyMfaChallengeCommandHandler(
    ILogger<VerifyMfaChallengeCommandHandler> logger,
    IMfaLoginChallengeRepository challengeRepository,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IRecoveryCodeGenerator recoveryCodeGenerator,
    IPasswordHasher passwordHasher,
    IOptions<MfaLockoutPolicy> lockoutPolicy) : ICommandHandler<VerifyMfaChallengeCommand, MfaChallengeVerification>
{
    private readonly MfaLockoutPolicy _policy = lockoutPolicy.Value.Normalize();

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
            {
                logger.LogWarning(
                    "Rejected MFA submission: challenge {ChallengeId} for user {UserId} is {Status}",
                    challenge.Id.Value, userId, challenge.Status);
                return Failure(userId, MfaFailureReason.UnknownChallenge);
            }

            if (challenge.Status == MfaChallengeStatus.Locked)
            {
                logger.LogWarning(
                    "Rejected MFA submission: challenge {ChallengeId} for user {UserId} is {Status}",
                    challenge.Id.Value, userId, challenge.Status);
                return Failure(userId, MfaFailureReason.ChallengeLocked);
            }

            if (challenge.IsExpired(now))
            {
                challenge.RegisterFailedAttempt(now);
                await challengeRepository.UpdateAsync(challenge, cancellationToken);
                return Failure(userId, MfaFailureReason.ChallengeExpired);
            }

            var credential = await credentialRepository.GetByUserIdAsync(challenge.UserId, cancellationToken);
            if (credential is null || !credential.IsEnabled)
                return Failure(userId, MfaFailureReason.NotEnrolled);

            // The per-credential budget outranks the per-challenge one: retyping the password buys a
            // fresh challenge with a fresh counter, so without this check the code space is
            // unbounded no matter how small MaxAttempts is.
            if (credential.IsLockedOut(now))
            {
                var remaining = _policy.RemainingLockout(now, credential.LockedUntilUtc);
                logger.LogWarning(
                    "Rejected MFA submission for user {UserId}: locked out for another {Remaining} after {Max} wrong codes",
                    userId, remaining, _policy.MaxAttemptsPerWindow);

                // The attempt is not spent here - the user already paid for it with the wrong codes
                // that installed the lockout.
                return new MfaChallengeVerification(
                    Succeeded: false,
                    UserId: userId,
                    IsLocked: true,
                    UsedRecoveryCode: false,
                    RequiresEnrollment: false,
                    AttemptsRemaining: 0,
                    ReturnUrl: null,
                    FailureReason: MfaFailureReason.LockedOut,
                    RetryAfterSeconds: (int)Math.Ceiling(remaining.TotalSeconds));
            }

            bool totpVerified;
            try
            {
                totpVerified = TryVerifyTotp(credential, command.Code, now);
            }
            catch (MfaSecretUnavailableException ex)
            {
                // The stored secret was encrypted with a Data Protection key that no longer
                // exists. No code can ever match, so the credential is demoted to "not enrolled"
                // and the user is sent through enrolment instead of getting a 500.
                logger.LogError(ex, "MFA secret for user {UserId} is undecryptable; requiring re-enrollment", userId);
                credential.RequireReenrollment();
                await credentialRepository.UpdateAsync(credential, cancellationToken);

                return new MfaChallengeVerification(
                    Succeeded: false,
                    UserId: userId,
                    IsLocked: false,
                    UsedRecoveryCode: false,
                    RequiresEnrollment: true,
                    AttemptsRemaining: 0,
                    ReturnUrl: null,
                    FailureReason: MfaFailureReason.SecretUnavailable,
                    RetryAfterSeconds: 0);
            }

            // Recovery codes are only considered when the entry was not a valid TOTP, so a
            // six-digit TOTP can never be mistaken for a recovery code.
            var usedRecoveryCode = !totpVerified
                && await TryConsumeRecoveryCodeAsync(credential, command.Code, now, cancellationToken);

            if (!totpVerified && !usedRecoveryCode)
                return await RejectAttemptAsync(challenge, credential, userId, now, cancellationToken);

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
                FailureReason: MfaFailureReason.None,
                RetryAfterSeconds: 0);
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
        MfaCredential credential,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // The credential-level budget is charged first: if it runs out the user is locked out for
        // the whole policy window, and the per-challenge result only decides what the Mfa page
        // shows right now.
        var credentialLockedOut = credential.RegisterFailedAttempt(now, _policy);
        await credentialRepository.UpdateAsync(credential, cancellationToken);

        var stillUsable = challenge.RegisterFailedAttempt(now);
        await challengeRepository.UpdateAsync(challenge, cancellationToken);

        if (credentialLockedOut)
        {
            logger.LogWarning(
                "MFA for user {UserId} is locked out until {LockedUntilUtc} after {Max} wrong codes inside the attempt window",
                userId, credential.LockedUntilUtc, _policy.MaxAttemptsPerWindow);

            return new MfaChallengeVerification(
                Succeeded: false,
                UserId: userId,
                IsLocked: true,
                UsedRecoveryCode: false,
                RequiresEnrollment: false,
                AttemptsRemaining: 0,
                ReturnUrl: null,
                FailureReason: MfaFailureReason.LockedOut,
                RetryAfterSeconds: (int)Math.Ceiling(_policy.LockoutDuration.TotalSeconds));
        }

        logger.LogWarning(
            "MFA challenge {ChallengeId} rejected a code for user {UserId} (attempt {Attempts}/{MaxAttempts})",
            challenge.Id.Value, userId, challenge.Attempts, MfaLoginChallenge.MaxAttempts);

        // Whichever budget is smaller is the one the user is about to hit.
        var attemptsRemaining = Math.Min(
            MfaLoginChallenge.MaxAttempts - challenge.Attempts,
            credential.AttemptsRemainingInWindow(now, _policy));

        return stillUsable
            ? Failure(userId, MfaFailureReason.InvalidCode, challenge.Attempts, attemptsRemaining)
            : Failure(userId, MfaFailureReason.ChallengeLocked, challenge.Attempts, 0);
    }

    private static MfaChallengeVerification Failure(Guid userId, int reason, int attempts = 0, int? attemptsRemaining = null) => new(
        Succeeded: false,
        UserId: userId,
        IsLocked: reason is MfaFailureReason.ChallengeLocked,
        UsedRecoveryCode: false,
        RequiresEnrollment: false,
        AttemptsRemaining: attemptsRemaining ?? Math.Max(0, MfaLoginChallenge.MaxAttempts - attempts),
        ReturnUrl: null,
        FailureReason: reason,
        RetryAfterSeconds: 0);
}