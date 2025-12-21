// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface IRepository<TAggregate, TId> where TAggregate : class, IAggregateRoot
{
  DbSet<TAggregate> CurrentContext { get; }
  Task AddAsync(TAggregate entity, CancellationToken cancellationToken);
  Task UpdateAsync(TAggregate entity, CancellationToken cancellationToken);
  Task DeleteAsync(TAggregate entity, CancellationToken cancellationToken);
  Task<TAggregate?> GetByIdAsync(Guid id);
  Task<IEnumerable<TAggregate>> GetAllAsync();
  Task<IEnumerable<TAggregate>> FindAsync(Expression<Func<TAggregate, bool>> predicate);
}
