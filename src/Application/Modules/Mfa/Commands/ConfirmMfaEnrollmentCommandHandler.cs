// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using OroIdentityServer.Core.Shared;

/// <summary>
/// Turns a pending enrolment into a working second factor: the first code from the authenticator
/// must match the stored secret, the credential is enabled, and a fresh set of single-use
/// recovery codes is issued (the previous ones are dropped, since a rotation invalidates them).
/// </summary>
public sealed class ConfirmMfaEnrollmentCommandHandler(
    ILogger<ConfirmMfaEnrollmentCommandHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IRecoveryCodeGenerator recoveryCodeGenerator) : ICommandHandler<ConfirmMfaEnrollmentCommand, RecoveryCodesResult>
{
    public async Task<RecoveryCodesResult> HandleAsync(ConfirmMfaEnrollmentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ConfirmMfaEnrollmentCommand for user {UserId}", command.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(command.UserId), cancellationToken)
                ?? throw new InvalidOperationException("MFA enrolment was never started for this user.");

            var secret = secretProtector.Unprotect(credential.EncryptedSecret);
            var now = DateTime.UtcNow;

            if (!totpService.TryVerifyCode(secret, command.Code, now, out var timeStep))
                throw new InvalidOperationException("The confirmation code is not valid.");

            credential.Confirm(now, timeStep);
            await credentialRepository.UpdateAsync(credential, cancellationToken);

            var plainCodes = await ReplaceRecoveryCodesAsync(credential, command.UserId, cancellationToken);

            logger.LogInformation("Confirmed MFA enrolment for user {UserId}", command.UserId);
            return new RecoveryCodesResult(plainCodes);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Rejected MFA enrolment for user {UserId}: {Reason}", command.UserId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming MFA enrolment for user {UserId}", command.UserId);
            throw;
        }
    }

    private async Task<IReadOnlyList<string>> ReplaceRecoveryCodesAsync(
        MfaCredential credential,
        Guid userId,
        CancellationToken cancellationToken)
    {
        foreach (var stale in await recoveryCodeRepository.GetByCredentialIdAsync(credential.Id, cancellationToken))
        {
            await recoveryCodeRepository.DeleteAsync(stale, cancellationToken);
        }

        var plainCodes = new List<string>();
        foreach (var generated in recoveryCodeGenerator.Generate())
        {
            plainCodes.Add(generated.PlainCode);
            await recoveryCodeRepository.AddAsync(
                new MfaRecoveryCode(credential.Id, new(userId), generated.CodeHash),
                cancellationToken);
        }

        return plainCodes;
    }
}