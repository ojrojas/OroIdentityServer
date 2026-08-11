// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class TenantRepository(
    ILogger<TenantRepository> logger,
    IRepository<Tenant> repository) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
        var result = await repository.FirstOrDefaultAsync(new GetTenantByIdSpecification(id), cancellationToken);
        logger.LogInformation("Exiting GetByIdAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await repository.ListAsync(new GetAllTenantsSpecification(), cancellationToken);
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindAsync");
        var result = await repository.FindAsync(predicate, cancellationToken);
        logger.LogInformation("Exiting FindAsync");
        return result;
    }

    public async Task<Tenant?> FindSingleAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering FindSingleAsync");
        var result = await repository.FindSingleAsync(predicate, cancellationToken);
        logger.LogInformation("Exiting FindSingleAsync");
        return result;
    }

    public async Task AddAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await repository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task UpdateAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateAsync");
        await repository.UpdateAsync(entity, cancellationToken);
        logger.LogInformation("Exiting UpdateAsync");
    }

    public async Task DeleteAsync(Tenant entity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync");
        ArgumentNullException.ThrowIfNull(entity);
        await repository.DeleteAsync(entity, cancellationToken);
        logger.LogInformation("Exiting DeleteAsync");
    }

    public async Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken ct)
    {
        logger.LogInformation("Entering GetBySlugAsync with slug: {Slug}", slug.Value);
        var result = await repository.FirstOrDefaultAsync(new GetTenantBySlugSpecification(slug), ct);
        logger.LogInformation("Exiting GetBySlugAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> GetByUserIdAsync(UserId userId, CancellationToken ct)
    {
        logger.LogInformation("Entering GetByUserIdAsync with userId: {UserId}", userId.Value);
        var result = await repository.ListAsync(new GetTenantsByUserIdSpecification(userId), ct);
        logger.LogInformation("Exiting GetByUserIdAsync");
        return result;
    }

    public async Task<int> CountCreatedTodayAsync(DateTime today, CancellationToken cancellationToken)
    {
        logger.LogInformation("Counting tenants created since {Today}", today);
        return await repository.CountAsync(new GetTenantsCreatedTodaySpecification(today), cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(TenantSlug slug, CancellationToken ct)
    {
        logger.LogInformation("Entering SlugExistsAsync with slug: {Slug}", slug.Value);
        var result = await repository.AnyAsync(new TenantSlugExistsSpecification(slug), ct);
        logger.LogInformation("Exiting SlugExistsAsync");
        return result;
    }
}
