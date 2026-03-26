// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to create a new role in the system.
/// </summary>
public record CreateRoleCommand(RoleName RoleName) : ICommand
{
    /// <summary>
    /// Gets the unique correlation identifier for this command instance.
    /// </summary>
    public Guid CorrelationId() => Guid.NewGuid();
}
