// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Services.OroIdentityServer.Infraestructure.Data;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class TenantRepository(
    ILogger<TenantRepository> logger,
    IRepository<Tenant> repository,
    OroIdentityAppContext context) : ITenantRepository
{
    public async Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddTenantAsync");
        await repository.AddAsync(tenant, cancellationToken);
        logger.LogInformation("Exiting AddTenantAsync");
    }

    public async Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateTenantAsync");
        await repository.UpdateAsync(tenant, cancellationToken);
        logger.LogInformation("Exiting UpdateTenantAsync");
    }

    public async Task DeleteTenantAsync(TenantId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteTenantAsync with id: {Id}", id);
        var tenant = await repository.GetByIdAsync(id, cancellationToken);
        if (tenant != null)
        {
            await repository.DeleteAsync(tenant, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteTenantAsync");
    }

    public async Task<Tenant?> GetTenantByIdAsync(TenantId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetTenantByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id, cancellationToken);
        logger.LogInformation("Exiting GetTenantByIdAsync");
        return result;
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllTenantsAsync");
        var result = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllTenantsAsync");
        return result;
    }
}
