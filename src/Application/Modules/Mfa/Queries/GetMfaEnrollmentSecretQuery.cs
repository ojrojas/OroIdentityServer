// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

using Microsoft.Extensions.Configuration;
using OroIdentityServer.Core.Modules.Mfa.Exceptions;

/// <summary>Plaintext secret plus the provisioning URI to render; only valid mid-enrolment.</summary>
public sealed record MfaSecretPayload(string ProvisioningUri, string Secret);

public record GetMfaEnrollmentSecretQuery(Guid UserId) : IQuery<MfaSecretPayload>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

/// <summary>
/// Rebuilds the provisioning URI from the stored (encrypted) secret so the QR can be rendered
/// again after a page reload, without rotating the secret underneath the user.
/// </summary>
public sealed class GetMfaEnrollmentSecretQueryHandler(
    ILogger<GetMfaEnrollmentSecretQueryHandler> logger,
    IMfaCredentialRepository credentialRepository,
    ITotpService totpService,
    IMfaSecretProtector secretProtector,
    IConfiguration configuration) : IQueryHandler<GetMfaEnrollmentSecretQuery, MfaSecretPayload>
{
    public async Task<MfaSecretPayload> HandleAsync(GetMfaEnrollmentSecretQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetMfaEnrollmentSecretQuery for user {UserId}", query.UserId);

        try
        {
            var credential = await credentialRepository.GetByUserIdAsync(new(query.UserId), cancellationToken)
                ?? throw new InvalidOperationException("MFA enrolment was never started for this user.");

            string secret;
            try
            {
                secret = secretProtector.Unprotect(credential.EncryptedSecret);
            }
            catch (MfaSecretUnavailableException ex)
            {
                // The QR cannot be rebuilt from a secret nobody can read. Report it as a plain
                // "never started" condition so the caller re-runs StartMfaEnrollment, which issues
                // a fresh secret.
                logger.LogError(ex, "MFA enrolment secret for user {UserId} is undecryptable; enrolment must restart", query.UserId);
                throw new InvalidOperationException("MFA enrolment was never started for this user.", ex);
            }

            var issuer = configuration["Branding:FullName"] ?? "OroIdentityServer";

            return new MfaSecretPayload(
                totpService.GetProvisioningUri(issuer, query.UserId.ToString(), secret),
                secret);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading the MFA enrolment secret for user {UserId}", query.UserId);
            throw;
        }
    }
}