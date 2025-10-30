// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class Repository<T>(
    ILogger<Repository<T>> logger,
    OroIdentityAppContext context) 
    : IRepository<T> where T : BaseEntity<Guid>, IAggregateRoot
{
    public DbSet<T> CurrentContext => context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
        var result = await context.Set<T>().FindAsync(id);
        logger.LogInformation("Exiting GetByIdAsync");
        return result;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await context.Set<T>().ToListAsync();
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        logger.LogInformation("Entering FindAsync");
        var result = await context.Set<T>().Where(predicate).ToListAsync();
        logger.LogInformation("Exiting FindAsync");
        return result;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await context.Set<T>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateAsync");
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting UpdateAsync");
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync");
        ArgumentNullException.ThrowIfNull(entity);
        (entity as BaseEntity<Guid>).State = EntityBaseState.DELETED;
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }
}