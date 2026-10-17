// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Creates (or rotates) the TOTP secret for a user and leaves the factor in a not-yet-confirmed
/// state: nothing is enforced until <see cref="ConfirmMfaEnrollmentCommand"/> accepts the first
/// code. Issuing a new secret silently invalidates whatever was there before, so this is also
/// the recovery path for a user who lost their phone.
/// </summary>
public sealed class StartMfaEnrollmentCommandHandler(
    ILogger<StartMfaEnrollmentCommandHandler> logger,
    IMfaCredentialRepository credentialRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IConfiguration configuration) : ICommandHandler<StartMfaEnrollmentCommand, MfaEnrollmentPayload>
{
    public async Task<MfaEnrollmentPayload> HandleAsync(StartMfaEnrollmentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling StartMfaEnrollmentCommand for user {UserId}", command.UserId);

        try
        {
            var secret = totpService.GenerateSecret();
            var protectedSecret = secretProtector.Protect(secret);

            var existing = await credentialRepository.GetByUserIdAsync(new(command.UserId), cancellationToken);

            // Rotating the secret of a working factor would disable MFA until the new one is
            // confirmed, so the caller has to go through the disable step on purpose.
            if (existing is { IsEnabled: true })
                throw new InvalidOperationException("MFA is already enabled for this user.");

            if (existing is null)
            {
                await credentialRepository.AddAsync(new MfaCredential(new(command.UserId), protectedSecret), cancellationToken);
            }
            else
            {
                existing.RestartEnrollment(protectedSecret);
                await credentialRepository.UpdateAsync(existing, cancellationToken);
            }

            // The issuer is what the authenticator app shows as the account name; the user id
            // keeps the entry unique even when two accounts share an email address.
            var issuer = configuration["Branding:FullName"] ?? "OroIdentityServer";
            var provisioningUri = totpService.GetProvisioningUri(issuer, command.UserId.ToString(), secret);

            logger.LogInformation("Started MFA enrolment for user {UserId}", command.UserId);
            return new MfaEnrollmentPayload(provisioningUri, secret, DateTime.UtcNow.AddMinutes(15));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting MFA enrolment for user {UserId}", command.UserId);
            throw;
        }
    }
}