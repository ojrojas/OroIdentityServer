// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>
/// Opens a challenge right after the password check succeeded and the caller decided MFA is due.
/// Any earlier open challenge for the same user is superseded, so a back button or a second tab
/// cannot resurrect a spent challenge.
/// </summary>
public sealed class CreateMfaLoginChallengeCommandHandler(
    ILogger<CreateMfaLoginChallengeCommandHandler> logger,
    IMfaLoginChallengeRepository challengeRepository) : ICommandHandler<CreateMfaLoginChallengeCommand, MfaLoginChallengeIssued>
{
    public async Task<MfaLoginChallengeIssued> HandleAsync(CreateMfaLoginChallengeCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling CreateMfaLoginChallengeCommand for user {UserId} (enrollment: {RequiresEnrollment})",
            command.UserId, command.RequiresEnrollment);

        try
        {
            var userId = new UserId(command.UserId);

            foreach (var open in await challengeRepository.GetOpenByUserIdAsync(userId, cancellationToken))
            {
                open.Supersede();
                await challengeRepository.UpdateAsync(open, cancellationToken);
            }

            var purpose = command.RequiresEnrollment ? MfaChallengePurpose.Enroll : MfaChallengePurpose.Verify;
            var challenge = new MfaLoginChallenge(
                userId,
                purpose,
                DateTime.UtcNow.AddMinutes(Math.Clamp(command.LifetimeMinutes, 1, 30)),
                command.ReturnUrl,
                command.IpAddress,
                command.UserAgent);

            await challengeRepository.AddAsync(challenge, cancellationToken);

            return new MfaLoginChallengeIssued(
                challenge.Token,
                challenge.Purpose == MfaChallengePurpose.Enroll,
                challenge.ReturnUrl,
                challenge.ExpiresAtUtc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating MFA login challenge for user {UserId}", command.UserId);
            throw;
        }
    }
}