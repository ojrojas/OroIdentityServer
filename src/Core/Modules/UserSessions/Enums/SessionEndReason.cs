// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserSessions.Enums;

/// <summary>
/// How a session stopped being usable. Needed to tell a genuine inactivity expiry (which the
/// user gets an email about) from an explicit sign-out or an administrator revocation, which
/// <c>DeactivateSession</c> used to write as the very same <c>ExpiresAt = UtcNow</c>.
/// </summary>
public enum SessionEndReason
{
    /// <summary>Still running.</summary>
    None = 0,

    /// <summary>Reached its expiry without enough activity to keep it alive.</summary>
    Expired = 1,

    /// <summary>The user signed out.</summary>
    LoggedOut = 2,

    /// <summary>An administrator terminated the session.</summary>
    RevokedByAdmin = 3
}