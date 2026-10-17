// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>
/// RFC 6238 time-based one-time passwords, as used by authenticator apps
/// (Google Authenticator, Microsoft Authenticator, 1Password, ...).
/// </summary>
public interface ITotpService
{
    /// <summary>A fresh 160 bit secret, base32 encoded (the format every authenticator expects).</summary>
    string GenerateSecret();

    /// <summary>
    /// The <c>otpauth://totp/...</c> URI to encode in the QR code. The issuer is the product
    /// name shown inside the app; the account label is what the user recognises.
    /// </summary>
    string GetProvisioningUri(string issuer, string accountLabel, string base32Secret);

    /// <summary>
    /// Verifies a code within the current time step plus one step of clock drift on each side.
    /// On success <paramref name="timeStep"/> carries the matched time step so the caller can
    /// reject a replay of an already-spent step.
    /// </summary>
    bool TryVerifyCode(string base32Secret, string code, DateTime utcNow, out long timeStep);

    /// <summary>The 30 second window a code belongs to at the given instant.</summary>
    long GetTimeStep(DateTime utcNow);
}