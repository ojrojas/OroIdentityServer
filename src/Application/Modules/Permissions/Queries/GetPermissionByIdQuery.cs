// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Queries;

public record GetPermissionByIdQuery(Guid PermissionId) : IQuery<GetPermissionByIdQueryResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
