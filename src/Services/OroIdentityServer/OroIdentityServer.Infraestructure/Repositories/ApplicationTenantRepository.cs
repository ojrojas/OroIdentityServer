// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Services.OroIdentityServer.Infraestructure.Data;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class ApplicationTenantRepository(
    ILogger<ApplicationTenantRepository> logger,
    IRepository<ApplicationTenant> repository,
    OroIdentityAppContext context) : IApplicationTenantRepository
{
    public async Task<TenantId?> GetTenantByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting tenant for clientId: {ClientId}", clientId);
        var appTenant = await repository.CurrentContext.FirstOrDefaultAsync(at => at.ClientId == clientId, cancellationToken: cancellationToken);
        return appTenant?.TenantId;
    }

    public async Task AddOrUpdateAsync(ApplicationTenant applicationTenant, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding/updating application tenant for client {ClientId}", applicationTenant.ClientId);
        var existing = await repository.CurrentContext.FirstOrDefaultAsync(at => at.ClientId == applicationTenant.ClientId, cancellationToken: cancellationToken);
        if (existing == null)
        {
            await repository.AddAsync(applicationTenant, cancellationToken);
        }
        else
        {
            existing.TenantId = applicationTenant.TenantId;
            await repository.UpdateAsync(existing, cancellationToken);
        }
    }
}
