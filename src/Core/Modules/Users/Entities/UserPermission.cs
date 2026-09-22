// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Users.Entities;

/// <summary>
/// Grants a domain permission directly to a user, independently of their roles. A user's
/// effective permissions are the union of these direct grants and the permissions inherited
/// from their active roles.
/// </summary>
public class UserPermission : IAggregateRoot
{
    private UserPermission()
    {
        UserId = default!;
        PermissionId = default!;
    }

    public UserPermission(UserId userId, PermissionId permissionId)
    {
        UserId = userId;
        PermissionId = permissionId;
    }

    public UserId UserId { get; private set; }
    public PermissionId PermissionId { get; private set; }
}
