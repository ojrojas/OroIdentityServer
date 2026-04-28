// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class TenantRepository(
    ILogger<TenantRepository> logger,
    IRepository<Tenant> repository,
    OroIdentityAppContext context) : ITenantRepository
{
     public DbSet<Tenant> CurrentContext => context.Set<Tenant>();

    public async Task<Tenant?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken) where TId : notnull
    {
        try
        {
            logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
            var result = await context.Set<Tenant>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
            logger.LogInformation("Exiting GetByIdAsync");
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await context.Set<Tenant>().ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindAsync");
        var result = await context.Set<Tenant>().Where(predicate).ToListAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Exiting FindAsync");
        return result;
    }

    public async Task<Tenant?> FindSingleAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindSingleAsync");
        var result = await context.Set<Tenant>().FirstOrDefaultAsync(predicate, cancellationToken);
        logger.LogInformation("Exiting FindSingleAsync");
        return result;
    }

    public async Task AddAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await context.Set<Tenant>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task UpdateAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateAsync");
        context.Set<Tenant>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting UpdateAsync");
    }

    public async Task DeleteAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync");
        ArgumentNullException.ThrowIfNull(entity);
        context.Set<Tenant>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }

    public async Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken ct)
    {
        logger.LogInformation("Entering GetBySlugAsync with slug: {Slug}", slug.Value);
        var result = await context.Set<Tenant>()
            .Include(t => t.TenantUsers)
            .FirstOrDefaultAsync(t => t.Slug.Value == slug.Value, ct);
        logger.LogInformation("Exiting GetBySlugAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> GetByUserIdAsync(UserId userId, CancellationToken ct)
    {
        logger.LogInformation("Entering GetByUserIdAsync with userId: {UserId}", userId.Value);
        var result = await context.Set<TenantUser>()
            .Where(tu => tu.UserId == userId && tu.IsActive)
            .Select(tu => tu.TenantId)
            .Distinct()
            .Join(
                context.Set<Tenant>(),
                tenantId => tenantId,
                tenant => tenant.Id,
                (_, tenant) => tenant)
            .ToListAsync(ct);
        logger.LogInformation("Exiting GetByUserIdAsync");
        return result;
    }

    public async Task<bool> SlugExistsAsync(TenantSlug slug, CancellationToken ct)
    {
        logger.LogInformation("Entering SlugExistsAsync with slug: {Slug}", slug.Value);
        var result = await context.Set<Tenant>()
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Slug.Value == slug.Value, ct);
        logger.LogInformation("Exiting SlugExistsAsync");
        return result;
    }
}
