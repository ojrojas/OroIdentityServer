namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
  Task AddAsync(T entity);
  Task UpdateAsync(T entity);
  Task DeleteAsync(T entity);
  Task<T?> GetByIdAsync(Guid id);
  Task<IEnumerable<T>> GetAllAsync();
  Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}