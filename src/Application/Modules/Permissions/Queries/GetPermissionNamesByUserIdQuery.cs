// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Queries;

/// <summary>
/// Resolves the distinct names of the domain permissions granted to a user through their
/// active roles. These names are emitted as <c>permission</c> claims.
/// </summary>
public record GetPermissionNamesByUserIdQuery(Guid UserId) : IQuery<GetPermissionNamesByUserIdQueryResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record GetPermissionNamesByUserIdQueryResponse : BaseResponse<IReadOnlyCollection<string>>
{
}
