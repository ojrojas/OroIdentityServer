// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for license information.
namespace OroIdentityServer.Core.Modules.Users.Repositories;

/// <summary>
/// Manages the permissions granted directly to a user (independent of roles).
/// </summary>
public interface IUserPermissionsRepository
{
    Task AddUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken);
    Task DeleteUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken);
    Task DeletePermissionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);
    Task<IEnumerable<UserPermission>> GetPermissionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}
