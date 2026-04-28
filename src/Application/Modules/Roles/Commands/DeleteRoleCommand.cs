// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Modules.Roles.Commands;

/// <summary>
/// Represents a command to delete a role by its ID.
/// </summary>
public record DeleteRoleCommand(Guid Id) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}