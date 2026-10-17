// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

/// <summary>What the security page needs to render the user's MFA state.</summary>
public sealed record MfaStatusResponse
{
    public bool IsEnabled { get; init; }
    public bool HasPendingEnrollment { get; init; }
    public bool TenantRequiresMfa { get; init; }
    public int UnusedRecoveryCodes { get; init; }
    public DateTime? ConfirmedAtUtc { get; init; }
    public DateTime? LastVerifiedAtUtc { get; init; }
}