// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Commands;

/// <summary>
/// Command to delete a permission.
/// </summary>
public record DeletePermissionCommand(
    Guid PermissionId
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
