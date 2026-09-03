// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Core.Modules.Hierarchy.Entities;

public sealed class UserReportingRelationship : Entity<UserReportingRelationshipId>, IAggregateRoot, IAuditableEntity
{
    public TenantId TenantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public UserId ReportsToUserId { get; private set; } = null!;
    public RelationshipType RelationshipType { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public User? ReportsToUser { get; private set; }
    public Tenant? Tenant { get; private set; }

    private UserReportingRelationship() { }

    public UserReportingRelationship(
        TenantId tenantId,
        UserId userId,
        UserId reportsToUserId,
        RelationshipType relationshipType,
        int priority,
        UserId? createdByUserId = null)
    {
        Id = UserReportingRelationshipId.New();
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ReportsToUserId = reportsToUserId ?? throw new ArgumentNullException(nameof(reportsToUserId));
        RelationshipType = relationshipType;
        Priority = priority;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;

        Validate();
    }

    public void Validate()
    {
        if (UserId.Value == ReportsToUserId.Value)
            throw new InvalidOperationException("cannot be own superior: User cannot report to themselves");
        if (Priority < 1)
            throw new ArgumentOutOfRangeException(nameof(Priority), "Priority must be >= 1");
    }

    public void UpdatePriority(int newPriority)
    {
        if (newPriority < 1)
            throw new ArgumentOutOfRangeException(nameof(newPriority), "Priority must be >= 1");
        Priority = newPriority;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
