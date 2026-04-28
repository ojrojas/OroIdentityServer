// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Entities;

public sealed class UserCompanyPreference : BaseEntity<UserCompanyPreference, UserCompanyPreferenceId>
{
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public ChartView DefaultChartView { get; private set; }
    public ReportPeriod DefaultReportPeriod { get; private set; }

    public UserCompanyPreference(
        Guid tenantId,
        string userId,
        Guid companyId,
        ChartView defaultChartView = ChartView.Tree,
        ReportPeriod defaultReportPeriod = ReportPeriod.Monthly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Id = UserCompanyPreferenceId.New();
        TenantId = tenantId;
        UserId = userId;
        CompanyId = companyId;
        DefaultChartView = defaultChartView;
        DefaultReportPeriod = defaultReportPeriod;
    }

    public void Update(ChartView chartView, ReportPeriod reportPeriod)
    {
        DefaultChartView = chartView;
        DefaultReportPeriod = reportPeriod;
    }

    private UserCompanyPreference() { }
}
