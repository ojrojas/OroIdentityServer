// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class Repository<T>(
    ILogger<Repository<T>> logger,
    OroIdentityAppContext context)
    : IRepository<T> where T : class, IAggregateRoot
{
    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken) where TId : notnull
    {
        try
        {
            logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
            var result = await context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
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

    public async Task<IEnumerable<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering ListAsync");
        var result = await SpecificationEvaluator.GetQuery(context.Set<T>().AsQueryable(), specification)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Exiting ListAsync");
        return result;
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FirstOrDefaultAsync");
        var result = await SpecificationEvaluator.GetQuery(context.Set<T>().AsQueryable(), specification)
            .FirstOrDefaultAsync(cancellationToken);
        logger.LogInformation("Exiting FirstOrDefaultAsync");
        return result;
    }

    public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AnyAsync");
        var query = context.Set<T>().AsQueryable();
        if (specification.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }
        var result = await query.AnyAsync(specification.ToExpression(), cancellationToken);
        logger.LogInformation("Exiting AnyAsync");
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

        // The entity may already be tracked by the current DbContext (e.g. a long-lived
        // scoped context that accumulated state across operations). Calling DbSet.Update on a
        // tracked entity is harmless, but calling it on a detached instance while another
        // instance with the same key is tracked throws an InvalidOperationException. Detect
        // that case and copy the incoming values into the tracked instance instead.
        var entry = context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            var tracked = FindTrackedByKey(entity);
            if (tracked is not null)
            {
                context.Entry(tracked).CurrentValues.SetValues(entity);
            }
            else
            {
                context.Set<T>().Update(entity);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting UpdateAsync");
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync");
        ArgumentNullException.ThrowIfNull(entity);
        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }

    private T? FindTrackedByKey(T entity)
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        var key = entityType?.FindPrimaryKey();
        if (entityType is null || key is null)
            return default;

        var properties = key.Properties;
        var entityEntry = context.Entry(entity);
        var keyValues = properties.Select(p => entityEntry.Property(p.Name).CurrentValue).ToArray();

        foreach (var candidate in context.Set<T>().Local)
        {
            var candidateEntry = context.Entry(candidate);
            var candidateValues = properties.Select(p => candidateEntry.Property(p.Name).CurrentValue).ToArray();
            if (candidateValues.SequenceEqual(keyValues))
                return candidate;
        }

        return default;
    }
}