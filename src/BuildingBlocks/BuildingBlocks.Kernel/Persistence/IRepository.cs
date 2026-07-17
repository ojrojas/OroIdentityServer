using System.Linq.Expressions;
using BuildingBlocks.Kernel.Domain;

namespace BuildingBlocks.Kernel.Persistence;

public interface IRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    Task<TAggregate?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
    Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TAggregate>> FindAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TAggregate?> FindSingleAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task DeleteAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}

public interface IRepository<TAggregate, TId>
    where TAggregate : class, IAggregateRoot
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
