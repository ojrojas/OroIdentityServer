// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Repositories;

/// <summary>
/// Repository for managing Tenant aggregates.
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken ct);
    Task<IEnumerable<Tenant>> GetByUserIdAsync(UserId userId, CancellationToken ct);
    Task<bool> SlugExistsAsync(TenantSlug slug, CancellationToken ct);
}
