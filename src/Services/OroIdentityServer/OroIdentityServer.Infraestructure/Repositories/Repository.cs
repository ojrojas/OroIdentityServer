// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class Repository<TAggregate, TId>(
    ILogger<Repository<TAggregate, TId>> logger,
    OroIdentityAppContext context)
    : IRepository<TAggregate, TId> where TAggregate : BaseEntity<Guid>, IAggregateRoot
{
    public DbSet<TAggregate> CurrentContext => context.Set<TAggregate>();

    public async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
            var result = await context.Set<TAggregate>().FindAsync(new object?[] { id, cancellationToken }, cancellationToken: cancellationToken);
            logger.LogInformation("Exiting GetByIdAsync");
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await context.Set<TAggregate>().ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<IEnumerable<TAggregate>> FindAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindAsync");
        var result = await context.Set<TAggregate>().Where(predicate).ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting FindAsync");
        return result;
    }

    public async Task<TAggregate?> FindSingleAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindSingleAsync");
        var result = await context.Set<TAggregate>().FirstOrDefaultAsync(predicate, cancellationToken);
        logger.LogInformation("Exiting FindSingleAsync");
        return result;
    }

    public async Task AddAsync(TAggregate entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await context.Set<TAggregate>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task UpdateAsync(TAggregate entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateAsync");
        context.Set<TAggregate>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting UpdateAsync");
    }

    public async Task DeleteAsync(TAggregate entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync");
        ArgumentNullException.ThrowIfNull(entity);
        context.Set<TAggregate>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }
}