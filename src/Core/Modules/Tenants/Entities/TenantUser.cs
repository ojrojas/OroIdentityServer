// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Entities;

/// <summary>
/// Membership row that records "user U belongs to tenant T". The tenant only needs to know
/// WHICH users it contains; the user's permissions live on <c>UserRole</c> (the catalogue
/// of roles) and <c>User.TenantId</c> for home-tenant checks. Carrying a per-tenant role
/// here was duplicating the catalogue and caused the two systems to drift out of sync.
/// </summary>
public sealed class TenantUser : Entity<TenantUserId>, IAggregateRoot
{
    public TenantId TenantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public UserId? PrimaryReportsToUserId { get; private set; }
    public int HierarchyLevel { get; private set; } = 10;

    // Navigation properties
    public User? PrimaryReportsToUser { get; private set; }

    public TenantUser(TenantId tenantId, UserId userId, UserId? primaryReportsToUserId = null, int hierarchyLevel = 10)
    {
        Id = TenantUserId.New();
        TenantId = tenantId;
        UserId = userId;
        IsActive = true;
        JoinedAtUtc = DateTime.UtcNow;
        PrimaryReportsToUserId = primaryReportsToUserId;
        HierarchyLevel = hierarchyLevel;
    }

    public void SetPrimaryReportsTo(UserId? primaryReportsToUserId)
    {
        PrimaryReportsToUserId = primaryReportsToUserId;
    }

    public void SetHierarchyLevel(int level)
    {
        if (level < 10 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "HierarchyLevel must be between 10 and 100");
        HierarchyLevel = level;
    }

    private TenantUser() { }
}
