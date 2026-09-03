// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Core.Modules.Hierarchy.DTOs;

public sealed record HierarchyRelationshipDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid ReportsToUserId,
    RelationshipType RelationshipType,
    int Priority,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record SuperiorDto(
    Guid UserId,
    string? UserName,
    string? Email,
    RelationshipType RelationshipType,
    int Priority,
    int HierarchyLevel,
    string? RoleName);

public sealed record SubordinateDto(
    Guid UserId,
    string? UserName,
    string? Email,
    RelationshipType RelationshipType,
    int Priority,
    int HierarchyLevel,
    string? RoleName);

public sealed record OrganizationTreeNodeDto(
    Guid UserId,
    string? UserName,
    string? Email,
    string? RoleName,
    int HierarchyLevel,
    List<OrganizationTreeNodeDto> Children,
    List<HierarchyRelationshipDto> SecondaryRelationships);

public sealed record HierarchyLevelDto(
    Guid UserId,
    Guid TenantId,
    int Level,
    string? RoleName);

public sealed record CanCommandResultDto(
    Guid CommanderId,
    Guid TargetId,
    bool CanCommand);
