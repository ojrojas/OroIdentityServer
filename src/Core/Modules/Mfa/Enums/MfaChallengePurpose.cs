// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>
/// Why a <c>MfaLoginChallenge</c> was issued: either the user already has a confirmed
/// credential and only has to prove it, or the login flow is forcing them to enrol
/// (tenant policy) before the session can be issued.
/// </summary>
public enum MfaChallengePurpose
{
    /// <summary>Prove possession of an already enrolled authenticator (TOTP or recovery code).</summary>
    Verify = 0,

    /// <summary>Enrol a brand new authenticator: show the QR, then confirm the first code.</summary>
    Enroll = 1
}