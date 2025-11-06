// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Command to delete a RoleClaim by its identifier.
/// </summary>
/// <param name="RoleClaimId">The unique identifier of the RoleClaim to delete.</param>
public record DeleteRoleClaimCommand(Guid RoleClaimId) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}