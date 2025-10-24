// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;


public interface IRolesRepository
{
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);
    Task UpdateRoleAsync(Role role, CancellationToken cancellationToken);
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken);
    Task<Role?> GetRoleByIdAsync(Guid id);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}