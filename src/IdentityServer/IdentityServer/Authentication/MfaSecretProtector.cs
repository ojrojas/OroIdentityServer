// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using Microsoft.AspNetCore.DataProtection;
using OroIdentityServer.Core.Interfaces;

/// <summary>
/// Encrypts the TOTP shared secret at rest. The plaintext secret is effectively a bearer
/// credential for the user's second factor, so the database only ever holds ciphertext.
/// </summary>
/// <remarks>
/// This depends on a persistent Data Protection key ring (configured in Program.cs). With the
/// default per-process ring, every restart would make every enrolled authenticator unusable.
/// </remarks>
public sealed class MfaSecretProtector : IMfaSecretProtector
{
    private readonly IDataProtector _protector;

    public MfaSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("oro.mfa.totp-secret.v1");
    }

    public string Protect(string plainSecret) => _protector.Protect(plainSecret);

    public string Unprotect(string protectedSecret) => _protector.Unprotect(protectedSecret);
}