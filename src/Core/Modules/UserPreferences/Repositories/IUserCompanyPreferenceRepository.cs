// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Repositories;

public interface IUserCompanyPreferenceRepository
{
    Task<UserCompanyPreference?> GetByUserAndCompanyAsync(string userId, Guid tenantId, Guid companyId, CancellationToken ct = default);
    Task<IReadOnlyList<UserCompanyPreference>> GetByUserAsync(string userId, Guid tenantId, CancellationToken ct = default);
    Task AddOrUpdateAsync(UserCompanyPreference entity, CancellationToken ct = default);
}
