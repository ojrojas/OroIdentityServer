// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

/// <summary>
/// Generic repository interface
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class, IAggregateRoot
{
  /// <summary>
  /// Gets the current DbContext Setted in T entity 
  /// </summary>
  DbSet<T> CurrentContext { get; }
  /// <summary>
  /// Adds a new entity to the repository
  /// </summary>
  /// <param name="entity"></param>
  /// <param name="cancellationToken"> Cancellation token </param>
  Task AddAsync(T entity, CancellationToken cancellationToken);
  /// <summary>
  /// Updates an existing entity in the repository
  /// </summary>
  /// <param name="entity"></param>
  /// <param name="cancellationToken"> Cancellation token </param>
  Task UpdateAsync(T entity, CancellationToken cancellationToken);
  /// <summary>
  /// Deletes an entity from the repository
  /// </summary>
  /// <param name="entity"></param>
  /// <param name="cancellationToken"> Cancellation token </param>
  Task DeleteAsync(T entity, CancellationToken cancellationToken);
  /// <summary>
  /// Gets an entity by its identifier
  /// </summary>
  /// <typeparam name="TId"></typeparam>
  /// <param name="id"></param>
  /// <param name="cancellationToken"> Cancellation token </param>
  /// <returns> Entity by identifier </returns>
  Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken) where TId : notnull;
  /// <summary>
  /// Gets all entities from the repository
  /// </summary>
  /// <param name="cancellationToken"> Cancellation token </param>
  /// <returns> All entities from the repository </returns>
  Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
  /// <summary>
  /// Finds entities based on a predicate
  /// </summary>
  /// <param name="predicate"> predicate query entity </param>
  /// <param name="cancellationToken"> Cancellation token </param>
  /// <returns> Entities matching the predicate </returns>
  Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
  /// <summary>
  /// Finds a single entity based on a predicate
  /// </summary>
  /// <param name="predicate"> predicate query entity </param>
  /// <param name="cancellationToken"> Cancellation token </param>
  /// <returns> Entity matching the predicate </returns>
  Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
}
