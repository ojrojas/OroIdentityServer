// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

/// <summary>
/// Represents a query to retrieve a role by its ID.
/// </summary>
public record GetRoleByIdQuery(RoleId Id) : IQuery<GetRoleByIdResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}