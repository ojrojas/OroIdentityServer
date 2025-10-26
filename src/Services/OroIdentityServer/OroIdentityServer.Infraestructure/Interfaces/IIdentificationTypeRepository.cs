// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

/// <summary>
/// Represents a repository interface for managing identification types.
/// </summary>
public interface IIdentificationTypeRepository
{
    /// <summary>
    /// Adds a new identification type to the repository asynchronously.
    /// </summary>
    /// <param name="identificationType">The identification type to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing identification type in the repository asynchronously.
    /// </summary>
    /// <param name="identificationType">The identification type to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an identification type from the repository asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the identification type to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteIdentificationTypeAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an identification type by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the identification type to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identification type if found; otherwise, null.</returns>
    Task<IdentificationType?> GetIdentificationTypeByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all identification types from the repository asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all identification types.</returns>
    Task<IEnumerable<IdentificationType>> GetAllIdentificationTypesAsync();
}