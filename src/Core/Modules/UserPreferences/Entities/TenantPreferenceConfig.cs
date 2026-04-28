// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Entities;

public sealed class TenantPreferenceConfig : BaseEntity<TenantPreferenceConfig, TenantPreferenceConfigId>
{
    public Guid TenantId { get; private set; }
    public Language DefaultLanguage { get; private set; }
    public Timezone DefaultTimezone { get; private set; }
    public DateFormat DefaultDateFormat { get; private set; }
    public NumberFormat DefaultNumberFormat { get; private set; }
    public AppTheme DefaultTheme { get; private set; }
    public bool ForceLanguage { get; private set; }
    public bool ForceTheme { get; private set; }

    public TenantPreferenceConfig(
        Guid tenantId,
        Language defaultLanguage = Language.Es,
        Timezone defaultTimezone = default,
        DateFormat defaultDateFormat = DateFormat.DdMmYyyy,
        NumberFormat defaultNumberFormat = NumberFormat.DotDecimal,
        AppTheme defaultTheme = AppTheme.System,
        bool forceLanguage = false,
        bool forceTheme = false)
    {
        Id = TenantPreferenceConfigId.New();
        TenantId = tenantId;
        DefaultLanguage = defaultLanguage;
        DefaultTimezone = defaultTimezone.Value is null ? Timezone.From("America/Bogota") : defaultTimezone;
        DefaultDateFormat = defaultDateFormat;
        DefaultNumberFormat = defaultNumberFormat;
        DefaultTheme = defaultTheme;
        ForceLanguage = forceLanguage;
        ForceTheme = forceTheme;
    }

    public void Update(
        Language defaultLanguage,
        Timezone defaultTimezone,
        DateFormat defaultDateFormat,
        NumberFormat defaultNumberFormat,
        AppTheme defaultTheme,
        bool forceLanguage,
        bool forceTheme)
    {
        DefaultLanguage = defaultLanguage;
        DefaultTimezone = defaultTimezone;
        DefaultDateFormat = defaultDateFormat;
        DefaultNumberFormat = defaultNumberFormat;
        DefaultTheme = defaultTheme;
        ForceLanguage = forceLanguage;
        ForceTheme = forceTheme;
    }

    private TenantPreferenceConfig() { }
}
