// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to update a role claim.
/// </summary>
public record UpdateRoleClaimCommand(Guid RoleClaimId, string ClaimType, string ClaimValue) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}