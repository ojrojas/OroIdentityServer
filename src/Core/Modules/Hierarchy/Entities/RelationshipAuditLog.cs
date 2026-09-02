// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Core.Modules.Hierarchy.Entities;

public enum RelationshipAuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
    PriorityChanged = 3
}

public sealed class RelationshipAuditLog : Entity<RelationshipAuditLogId>, IAuditableEntity
{
    public UserReportingRelationshipId RelationshipId { get; private set; } = null!;
    public TenantId TenantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public UserId ReportsToUserId { get; private set; } = null!;
    public RelationshipType RelationshipType { get; private set; }
    public RelationshipAuditAction Action { get; private set; }
    public UserId? PerformedByUserId { get; private set; }
    public DateTime PerformedAtUtc { get; private set; }
    public string? Details { get; private set; }
    public string? Reason { get; private set; }

    private RelationshipAuditLog() { }

    public RelationshipAuditLog(
        UserReportingRelationshipId relationshipId,
        TenantId tenantId,
        UserId userId,
        UserId reportsToUserId,
        RelationshipType relationshipType,
        RelationshipAuditAction action,
        UserId? performedByUserId = null,
        string? details = null,
        string? reason = null)
    {
        Id = RelationshipAuditLogId.New();
        RelationshipId = relationshipId ?? throw new ArgumentNullException(nameof(relationshipId));
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ReportsToUserId = reportsToUserId ?? throw new ArgumentNullException(nameof(reportsToUserId));
        RelationshipType = relationshipType;
        Action = action;
        PerformedByUserId = performedByUserId;
        PerformedAtUtc = DateTime.UtcNow;
        Details = details;
        Reason = reason;
    }
}
