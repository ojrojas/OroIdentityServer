// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Repositories;

public interface ITenantPreferenceConfigRepository
{
    Task<TenantPreferenceConfig?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TenantPreferenceConfig entity, CancellationToken ct = default);
}
