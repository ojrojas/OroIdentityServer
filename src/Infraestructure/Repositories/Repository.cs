// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class Repository<T>(
    ILogger<Repository<T>> logger,
    OroIdentityAppContext context)
    : IRepository<T> where T : class, IAggregateRoot
{
    public DbSet<T> CurrentContext => context.Set<T>();

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken) where TId : notnull
    {
         try
        {
            logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
            var result = await context.Set<T>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
            logger.LogInformation("Exiting GetByIdAsync");
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await context.Set<T>().ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindAsync");
        var result = await context.Set<T>().Where(predicate).ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting FindAsync");
        return result;
    }

    public async Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindSingleAsync");
        var result = await context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        logger.LogInformation("Exiting FindSingleAsync");
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
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }


}