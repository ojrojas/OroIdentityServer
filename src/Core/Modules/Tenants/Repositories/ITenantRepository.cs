// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Repositories;

/// <summary>
/// Repository for managing Tenant aggregates.
/// </summary>
public interface ITenantRepository
{
    Task AddAsync(Tenant entity, CancellationToken cancellationToken);
    Task DeleteAsync(Tenant entity, CancellationToken cancellationToken);
    Task<IEnumerable<Tenant>> FindAsync(System.Linq.Expressions.Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);
    Task<Tenant?> FindSingleAsync(System.Linq.Expressions.Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken);
    Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken ct);
    Task<IEnumerable<Tenant>> GetByUserIdAsync(UserId userId, CancellationToken ct);
    Task<bool> SlugExistsAsync(TenantSlug slug, CancellationToken ct);
    Task UpdateAsync(Tenant entity, CancellationToken cancellationToken);
}
