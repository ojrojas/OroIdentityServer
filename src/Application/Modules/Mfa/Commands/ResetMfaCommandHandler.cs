// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

public sealed class ResetMfaCommandHandler(
    ILogger<ResetMfaCommandHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository) : ICommandHandler<ResetMfaCommand>
{
    public async Task<Result> HandleAsync(ResetMfaCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ResetMfaCommand for user {UserId}", command.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(command.UserId), cancellationToken);
            if (credential is null)
            {
                logger.LogInformation("No MFA credential to reset for user {UserId}", command.UserId);
                return Result.Success();
            }

            credential.Disable();
            await credentialRepository.UpdateAsync(credential, cancellationToken);

            foreach (var code in await recoveryCodeRepository.GetByCredentialIdAsync(credential.Id, cancellationToken))
                await recoveryCodeRepository.DeleteAsync(code, cancellationToken);

            logger.LogInformation("Reset MFA credential {CredentialId} for user {UserId}", credential.Id.Value, command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting MFA for user {UserId}", command.UserId);
            throw;
        }
    }
}