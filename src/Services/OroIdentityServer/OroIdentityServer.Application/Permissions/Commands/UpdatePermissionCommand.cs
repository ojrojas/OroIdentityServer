// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Command to update an existing permission.
/// </summary>
public record UpdatePermissionCommand(
    PermissionId PermissionId,
    string Name,
    string DisplayName,
    string? Description,
    string Resource
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
