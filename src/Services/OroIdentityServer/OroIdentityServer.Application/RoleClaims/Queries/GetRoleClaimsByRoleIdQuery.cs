// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

/// <summary>
/// Query to retrieve RoleClaims by RoleId.
/// </summary>
/// <param name="RoleId">The unique identifier of the Role.</param>
public record GetRoleClaimsByRoleIdQuery(Guid RoleId) : IQuery<GetRoleClaimsByRoleIdResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
