// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>
/// Closes an enrolment challenge once the first code has been confirmed, handing the caller the
/// user id and the original return url so the login can finally issue the session.
/// </summary>
public record CompleteMfaEnrollmentChallengeCommand(string ChallengeToken) : ICommand<MfaChallengeVerification>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public sealed class CompleteMfaEnrollmentChallengeCommandHandler(
    ILogger<CompleteMfaEnrollmentChallengeCommandHandler> logger,
    IMfaLoginChallengeRepository challengeRepository) : ICommandHandler<CompleteMfaEnrollmentChallengeCommand, MfaChallengeVerification>
{
    public async Task<MfaChallengeVerification> HandleAsync(
        CompleteMfaEnrollmentChallengeCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CompleteMfaEnrollmentChallengeCommand");

        try
        {
            var challenge = await challengeRepository.GetByTokenAsync(command.ChallengeToken, cancellationToken);
            var now = DateTime.UtcNow;

            if (challenge is null)
                return Rejected(MfaFailureReason.UnknownChallenge);

            if (challenge.Purpose != MfaChallengePurpose.Enroll)
                return Rejected(MfaFailureReason.UnknownChallenge);

            if (challenge.Status is MfaChallengeStatus.Completed or MfaChallengeStatus.Superseded)
                return Rejected(MfaFailureReason.UnknownChallenge);

            if (challenge.Status == MfaChallengeStatus.Locked || challenge.IsExpired(now))
            {
                challenge.RegisterFailedAttempt(now);
                await challengeRepository.UpdateAsync(challenge, cancellationToken);
                return Rejected(MfaFailureReason.ChallengeExpired);
            }

            challenge.Complete(now);
            await challengeRepository.UpdateAsync(challenge, cancellationToken);

            return new MfaChallengeVerification(
                Succeeded: true,
                UserId: challenge.UserId.Value,
                IsLocked: false,
                UsedRecoveryCode: false,
                RequiresEnrollment: false,
                AttemptsRemaining: 0,
                ReturnUrl: challenge.ReturnUrl,
                FailureReason: MfaFailureReason.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing MFA enrolment challenge");
            throw;
        }
    }

    private static MfaChallengeVerification Rejected(int reason) => new(
        Succeeded: false,
        UserId: Guid.Empty,
        IsLocked: false,
        UsedRecoveryCode: false,
        RequiresEnrollment: false,
        AttemptsRemaining: 0,
        ReturnUrl: null,
        FailureReason: reason);
}