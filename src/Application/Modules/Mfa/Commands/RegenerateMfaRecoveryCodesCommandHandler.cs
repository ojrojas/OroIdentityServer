// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>Issues a new set of single-use recovery codes, invalidating the previous ones.</summary>
public sealed class RegenerateMfaRecoveryCodesCommandHandler(
    ILogger<RegenerateMfaRecoveryCodesCommandHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    IRecoveryCodeGenerator recoveryCodeGenerator) : ICommandHandler<RegenerateMfaRecoveryCodesCommand, RecoveryCodesResult>
{
    public async Task<RecoveryCodesResult> HandleAsync(RegenerateMfaRecoveryCodesCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling RegenerateMfaRecoveryCodesCommand for user {UserId}", command.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(command.UserId), cancellationToken)
                ?? throw new InvalidOperationException("This user has no MFA credential.");

            foreach (var stale in await recoveryCodeRepository.GetByCredentialIdAsync(credential.Id, cancellationToken))
                await recoveryCodeRepository.DeleteAsync(stale, cancellationToken);

            var plainCodes = new List<string>();
            foreach (var generated in recoveryCodeGenerator.Generate())
            {
                plainCodes.Add(generated.PlainCode);
                await recoveryCodeRepository.AddAsync(
                    new MfaRecoveryCode(credential.Id, new(command.UserId), generated.CodeHash),
                    cancellationToken);
            }

            return new RecoveryCodesResult(plainCodes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error regenerating MFA recovery codes for user {UserId}", command.UserId);
            throw;
        }
    }
}