// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Commands;

/// <summary>
/// Replaces the complete set of domain permissions assigned to a role with
/// <paramref name="PermissionIds"/>.
/// </summary>
public record SetRolePermissionsCommand(Guid RoleId, IReadOnlyCollection<Guid> PermissionIds) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
