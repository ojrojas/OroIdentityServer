// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Commands;

/// <summary>
/// Represents a command to update an existing role in the system.
/// </summary>
public record UpdateRoleCommand(Guid Id, string RoleName) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}