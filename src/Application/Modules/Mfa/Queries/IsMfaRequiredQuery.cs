// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

/// <summary>
/// Decides, right after the password check, whether a second factor is due. MFA is either the
/// user's own opt-in or the tenant's policy; in the second case a user who never enrolled is
/// required to enrol now rather than being locked out.
/// </summary>
public sealed record MfaRequirementResponse(bool IsRequired, bool RequiresEnrollment);

public record IsMfaRequiredQuery(Guid UserId) : IQuery<MfaRequirementResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}