// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to associate a claim to a role.
/// </summary>
public record AssociateClaimToRoleCommand(
    RoleId RoleId, 
    RoleClaimType ClaimType, 
    RoleClaimValue ClaimValue) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}