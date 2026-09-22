// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Commands;

/// <summary>
/// Replaces the complete set of domain permissions granted directly to a user.
/// </summary>
public sealed record AssignPermissionsToUserCommand(
    Guid UserId,
    List<Guid> PermissionIds
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
