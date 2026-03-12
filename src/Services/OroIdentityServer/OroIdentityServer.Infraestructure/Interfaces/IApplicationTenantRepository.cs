// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OroIdentityServer.Services.OroIdentityServer.Infraestructure.Data;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface IApplicationTenantRepository
{
    Task<TenantId?> GetTenantByClientIdAsync(string clientId, CancellationToken cancellationToken);
    Task AddOrUpdateAsync(ApplicationTenant applicationTenant, CancellationToken cancellationToken);
}
