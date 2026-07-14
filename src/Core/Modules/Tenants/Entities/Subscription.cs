// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Entities;

public sealed class Subscription : BaseEntity<Subscription, SubscriptionId>, IAuditableEntity
{
    public TenantId TenantId { get; private set; } = null!;
    public PlanType Plan { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public int MaxCompanies { get; private set; }
    public int MaxUsersPerCompany { get; private set; }

    public Subscription(TenantId tenantId, PlanType plan, int maxCompanies, int maxUsersPerCompany)
    {
        Id = SubscriptionId.New();
        TenantId = tenantId;
        Plan = plan;
        StartDate = DateTime.UtcNow;
        IsActive = true;
        MaxCompanies = maxCompanies;
        MaxUsersPerCompany = maxUsersPerCompany;
    }

    private Subscription() { }
}
