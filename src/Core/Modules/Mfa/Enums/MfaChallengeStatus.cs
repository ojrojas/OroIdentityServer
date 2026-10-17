// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>
/// Lifecycle of a <c>MfaLoginChallenge</c>. The row is kept after completion/failure so the
/// login flow can distinguish "already verified" from "never verified" and so the attempt
/// counter survives a browser reload.
/// </summary>
public enum MfaChallengeStatus
{
    /// <summary>Awaiting the code.</summary>
    Pending = 0,

    /// <summary>The code was accepted; the admin cookie was issued.</summary>
    Completed = 1,

    /// <summary>One or more codes were wrong but the challenge is still usable.</summary>
    Failed = 2,

    /// <summary>The attempt budget was exhausted; the challenge is dead.</summary>
    Locked = 3,

    /// <summary>Superseded by a newer challenge for the same user.</summary>
    Superseded = 4
}