// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Associates multiple permissions to a role in a single command.
/// Permission identifiers are PermissionId value objects.
/// </summary>
public record AssociatePermissionsToRoleCommand(RoleId RoleId, IEnumerable<PermissionId> PermissionIds) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
