// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>
/// Issues the "second factor pending" state after a successful password check. The returned
/// token is what the browser carries in the short-lived pending cookie; every code submission is
/// validated against the server-side challenge behind it.
/// </summary>
public sealed record MfaLoginChallengeIssued(
    string Token,
    bool RequiresEnrollment,
    string? ReturnUrl,
    DateTime ExpiresAtUtc);