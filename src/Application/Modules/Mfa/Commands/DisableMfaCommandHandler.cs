// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using OroIdentityServer.Core.Modules.Mfa.Exceptions;
using OroIdentityServer.Core.Modules.Users.Repositories;

/// <summary>
/// Turns the factor off. Deliberately demanding both the password and a current code: a stolen
/// session cookie must not be enough to strip the second factor from an account.
/// </summary>
public sealed class DisableMfaCommandHandler(
    ILogger<DisableMfaCommandHandler> logger,
    IMfaCredentialRepository credentialRepository,
    IMfaRecoveryCodeRepository recoveryCodeRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IUserRepository userRepository,
    ISecurityUserRepository securityUserRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<DisableMfaCommand>
{
    public async Task<Result> HandleAsync(DisableMfaCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DisableMfaCommand for user {UserId}", command.UserId);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken);
            if (user?.SecurityUserId is null)
                return Result.Failure(Error.NotFound("UserNotFound", "The user was not found."));

            var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId.Value, cancellationToken);
            if (securityUser?.PasswordHash is null || !await passwordHasher.VerifyPassword(command.Password, securityUser.PasswordHash))
                return Result.Failure(Error.Unauthorized("InvalidPassword", "The password is not valid."));

            var credential = await credentialRepository.GetByUserIdAsync(new(command.UserId), cancellationToken);
            if (credential is null)
                return Result.Failure(Error.NotFound("MfaNotEnrolled", "This user has no MFA credential."));

            string secret;
            try
            {
                secret = secretProtector.Unprotect(credential.EncryptedSecret);
            }
            catch (MfaSecretUnavailableException ex)
            {
                // Nothing to prove: the factor cannot be verified because its secret is gone. Turn
                // it off and say so, instead of 500-ing on a page the user reached deliberately.
                logger.LogError(ex, "MFA secret for user {UserId} is undecryptable; disabling the factor", command.UserId);
                credential.RequireReenrollment();
                await credentialRepository.UpdateAsync(credential, cancellationToken);

                foreach (var stale in await recoveryCodeRepository.GetByCredentialIdAsync(credential.Id, cancellationToken))
                    await recoveryCodeRepository.DeleteAsync(stale, cancellationToken);

                return Result.Failure(Error.Conflict(
                    "MfaSecretUnavailable",
                    "The authenticator secret is no longer recoverable. MFA has been turned off; enrol a new authenticator."));
            }

            if (!totpService.TryVerifyCode(secret, command.Code, DateTime.UtcNow, out _))
                return Result.Failure(Error.Unauthorized("InvalidMfaCode", "The authentication code is not valid."));

            credential.Disable();
            await credentialRepository.UpdateAsync(credential, cancellationToken);

            foreach (var code in await recoveryCodeRepository.GetByCredentialIdAsync(credential.Id, cancellationToken))
                await recoveryCodeRepository.DeleteAsync(code, cancellationToken);

            logger.LogInformation("Disabled MFA for user {UserId}", command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disabling MFA for user {UserId}", command.UserId);
            throw;
        }
    }
}