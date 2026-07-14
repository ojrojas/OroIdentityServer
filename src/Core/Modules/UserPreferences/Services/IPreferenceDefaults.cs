// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Services;

public interface IPreferenceDefaults
{
    Task<(Language Language, Timezone Timezone, DateFormat DateFormat, NumberFormat NumberFormat, AppTheme Theme)>
        GetDefaultsAsync(string language, string region, Guid tenantId, CancellationToken ct = default);
}
