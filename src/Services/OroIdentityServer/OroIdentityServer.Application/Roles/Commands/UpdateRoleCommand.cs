// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to update an existing role in the system.
/// </summary>
public record UpdateRoleCommand(Guid Id, RoleName RoleName) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}