// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface IUserRolesRepository
{
    Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
    Task DeleteRolesByUserIdAsync(UserId userId, CancellationToken cancellationToken);
    Task DeleteUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
    Task<IEnumerable<UserRole>> GetAllRolesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}
