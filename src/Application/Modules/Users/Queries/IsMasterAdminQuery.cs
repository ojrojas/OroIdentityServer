// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Queries;

/// <summary>
/// Is this user the master admin: a member of the <paramref name="MasterTenantName"/> tenant
/// carrying the Administrator (or Admin) catalogue role. The answer drives the
/// <c>is_master_admin</c> claim and the <c>MasterAdminOnly</c> policy.
/// </summary>
/// <remarks>
/// Lives here, not in the sign-in service, so the rule that decides who administers the
/// IdP console is owned by the Application layer and unit-testable without HTTP or cookies.
/// The tenant name travels as a parameter so the handler stays free of configuration.
/// </remarks>
public record IsMasterAdminQuery(Guid UserId, string MasterTenantName) : IQuery<IsMasterAdminResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public sealed record IsMasterAdminResponse : BaseResponse<bool>;
