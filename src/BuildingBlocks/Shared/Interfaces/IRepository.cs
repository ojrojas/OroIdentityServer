// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface IRepository<T> where T : BaseEntity<Guid>, IAggregateRoot
{
  DbSet<T> CurrentContext {get;}
  Task AddAsync(T entity, CancellationToken cancellationToken);
  Task UpdateAsync(T entity, CancellationToken cancellationToken);
  Task DeleteAsync(T entity, CancellationToken cancellationToken);
  Task<T?> GetByIdAsync(Guid id);
  Task<IEnumerable<T>> GetAllAsync();
  Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
