// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages roles.
/// </summary>
public interface IRolesRepository
{
    /// <summary>
    /// Asynchronously adds a new role to the repository.
    /// </summary>
    /// <param name="role">The role to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing role in the repository.
    /// </summary>
    /// <param name="role">The role to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateRoleAsync(Role role, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes a role from the repository by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a role by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role if found; otherwise, null.</returns>
    Task<Role?> GetRoleByIdAsync(Guid id);

    /// <summary>
    /// Asynchronously retrieves all roles from the repository.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of roles.</returns>
    Task<IEnumerable<Role>> GetAllRolesAsync();
}