// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>
/// Protects the TOTP shared secret at rest. The plaintext secret is a bearer credential for the
/// user's second factor, so it must never sit in the database unprotected.
/// </summary>
public interface IMfaSecretProtector
{
    string Protect(string plainSecret);
    string Unprotect(string protectedSecret);
}