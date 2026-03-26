// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

/// <summary>
/// Query to retrieve a RoleClaim by its identifier.
/// </summary>
/// <param name="Id">The unique identifier of the RoleClaim.</param>
public record GetRoleClaimByIdQuery(RoleClaimId Id) : IQuery<GetRoleClaimByIdResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
