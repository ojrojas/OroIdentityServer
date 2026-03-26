// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

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
    Task DeleteRoleAsync(RoleId id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a role by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role if found; otherwise, null.</returns>
    Task<Role?> GetRoleByIdAsync(RoleId id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves all roles from the repository.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of roles.</returns>
    Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a role claim by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role claim to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role claim if found; otherwise, null.</returns>
    Task<RoleClaim?> GetRoleClaimByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves all role claims associated with a specific role identifier.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of role claims.</returns>
    Task<IEnumerable<RoleClaim>> GetRoleClaimsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously adds a new role claim to a specific role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to add the claim to.</param>
    /// <param name="claimType">The type of the claim.</param>
    /// <param name="claimValue">The value of the claim.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddRoleClaimAsync(RoleId roleId, RoleClaimType claimType, RoleClaimValue claimValue, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes a role claim from the repository by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role claim to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing role claim in the repository.
    /// </summary>
    /// <param name="claimId">The unique identifier of the role claim to update.</param>
    /// <param name="newClaimType">The new type of the claim.</param>
    /// <param name="newClaimValue">The new value of the claim.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateRoleClaimAsync(Guid claimId, RoleClaimType newClaimType, RoleClaimValue newClaimValue, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a role by its name.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role if found; otherwise, null.</returns>
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
}