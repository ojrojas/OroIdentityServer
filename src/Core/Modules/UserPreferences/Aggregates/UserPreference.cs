// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Aggregates;

public sealed class UserPreference : BaseEntity<UserPreference, UserPreferenceId>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Language Language { get; private set; }
    public Timezone Timezone { get; private set; }
    public DateFormat DateFormat { get; private set; }
    public NumberFormat NumberFormat { get; private set; }
    public AppTheme Theme { get; private set; }
    public Guid? DefaultCompanyId { get; private set; }
    public InboxSortField InboxSortField { get; private set; }
    public SortDirection InboxSortDirection { get; private set; }
    public string? DashboardLayout { get; private set; }
    public bool SidebarCollapsed { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public UserPreference(
        Guid tenantId,
        string userId,
        Language language,
        Timezone timezone,
        DateFormat dateFormat,
        NumberFormat numberFormat,
        AppTheme theme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Id = UserPreferenceId.New();
        TenantId = tenantId;
        UserId = userId;
        Language = language;
        Timezone = timezone;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
        Theme = theme;
        InboxSortField = InboxSortField.AiPriority;
        InboxSortDirection = SortDirection.Descending;
        SidebarCollapsed = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserPreferenceCreatedDomainEvent(Id, UserId, TenantId, Language, Theme));
    }

    public void Update(Language language, Timezone timezone, DateFormat dateFormat, NumberFormat numberFormat, AppTheme theme)
    {
        Language = language;
        Timezone = timezone;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
        Theme = theme;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserPreferenceUpdatedDomainEvent(Id, UserId, TenantId));
    }

    public void SetDefaultCompany(Guid? companyId)
    {
        DefaultCompanyId = companyId;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new DefaultCompanySetDomainEvent(Id, UserId, companyId));
    }

    public void UpdateInboxSort(InboxSortField field, SortDirection direction)
    {
        InboxSortField = field;
        InboxSortDirection = direction;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDashboardLayout(string layout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layout);

        DashboardLayout = layout;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new DashboardLayoutUpdatedDomainEvent(Id, UserId));
    }

    public void UpdateSidebar(bool collapsed)
    {
        SidebarCollapsed = collapsed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reset(Language language, Timezone timezone, DateFormat dateFormat, NumberFormat numberFormat, AppTheme theme)
    {
        Language = language;
        Timezone = timezone;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
        Theme = theme;
        DefaultCompanyId = null;
        InboxSortField = InboxSortField.AiPriority;
        InboxSortDirection = SortDirection.Descending;
        DashboardLayout = null;
        SidebarCollapsed = false;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserPreferenceUpdatedDomainEvent(Id, UserId, TenantId));
    }

    private UserPreference() { }
}
