// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using Microsoft.AspNetCore.DataProtection;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Mfa.Exceptions;

/// <summary>
/// Encrypts the TOTP shared secret at rest. The plaintext secret is effectively a bearer
/// credential for the user's second factor, so the database only ever holds ciphertext.
/// </summary>
/// <remarks>
/// This depends on a persistent Data Protection key ring (see <see cref="DataProtectionKeyRingPath"/>).
/// A ring that is regenerated after enrolment makes every secret unreadable, so the raw
/// <see cref="System.Security.Cryptography.CryptographicException"/> is translated into
/// <see cref="MfaSecretUnavailableException"/> here - the one place that knows the difference -
/// instead of surfacing as an unhandled 500 on the MFA page.
/// </remarks>
public sealed class MfaSecretProtector : IMfaSecretProtector
{
    private readonly IDataProtector _protector;
    private readonly ILogger<MfaSecretProtector> _logger;

    public MfaSecretProtector(IDataProtectionProvider provider, ILogger<MfaSecretProtector> logger)
    {
        _protector = provider.CreateProtector("oro.mfa.totp-secret.v1");
        _logger = logger;
    }

    public string Protect(string plainSecret) => _protector.Protect(plainSecret);

    public string Unprotect(string protectedSecret)
    {
        try
        {
            return _protector.Unprotect(protectedSecret);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            _logger.LogError(
                ex,
                "The MFA secret could not be decrypted: the Data Protection key that produced it is no longer in the key ring. The credential must be re-enrolled.");
            throw new MfaSecretUnavailableException(
                "The stored MFA secret was encrypted with a key that is no longer available.", ex);
        }
    }
}