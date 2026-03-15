// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Core.Models;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface ITenantRepository
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken);
    Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken);
    Task DeleteTenantAsync(TenantId id, CancellationToken cancellationToken);
    Task<Tenant?> GetTenantByIdAsync(TenantId id, CancellationToken cancellationToken);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken);
}
