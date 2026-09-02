namespace IdentityServer.Client.Models.Hierarchy;

public sealed record HierarchyRelationshipModel(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid ReportsToUserId,
    string RelationshipType,
    int Priority,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateRelationshipRequest(Guid UserId, Guid ReportsToUserId, string Type, int Priority);
public sealed record UpdatePriorityRequest(int Priority);

public sealed record SuperiorModel(
    Guid UserId,
    string? UserName,
    string? Email,
    string RelationshipType,
    int Priority,
    int HierarchyLevel,
    string? RoleName);

public sealed record SubordinateModel(
    Guid UserId,
    string? UserName,
    string? Email,
    string RelationshipType,
    int Priority,
    int HierarchyLevel,
    string? RoleName);

public sealed record OrganizationTreeNodeModel(
    Guid UserId,
    string? UserName,
    string? Email,
    string? RoleName,
    int HierarchyLevel,
    List<OrganizationTreeNodeModel> Children,
    List<HierarchyRelationshipModel> SecondaryRelationships);

public sealed record HierarchyLevelModel(Guid UserId, int Level);
public sealed record CanCommandModel(Guid CommanderId, Guid TargetId, bool CanCommand);
