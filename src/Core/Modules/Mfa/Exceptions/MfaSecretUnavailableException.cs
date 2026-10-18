// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Exceptions;

/// <summary>
/// The stored TOTP secret cannot be decrypted with the current Data Protection key ring.
/// </summary>
/// <remarks>
/// This is not a transient failure and not a wrong code: the ciphertext is permanently
/// unreadable, so no number of retries helps. The usual cause is a key ring that was regenerated
/// (build output wiped, volume lost) after enrolment. Callers must surface "re-enrolment
/// required" instead of letting the raw <see cref="System.Security.Cryptography.CryptographicException"/>
/// escape as a 500 - a user who mistypes a code should never see a stack trace.
/// </remarks>
public sealed class MfaSecretUnavailableException : Exception
{
    public MfaSecretUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}