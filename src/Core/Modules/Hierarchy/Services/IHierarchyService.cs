// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using OroIdentityServer.Core.Modules.Hierarchy.DTOs;
using OroIdentityServer.Core.Modules.Hierarchy.Entities;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Core.Modules.Hierarchy.Services;

public interface IHierarchyService
{
    Task<UserReportingRelationship> CreateRelationshipAsync(
        TenantId tenantId,
        UserId userId,
        UserId reportsToUserId,
        RelationshipType type,
        int priority,
        UserId? performedByUserId = null,
        CancellationToken ct = default);

    Task<UserReportingRelationship> UpdateRelationshipPriorityAsync(
        UserReportingRelationshipId relationshipId,
        int newPriority,
        UserId? performedByUserId = null,
        CancellationToken ct = default);

    Task DeleteRelationshipAsync(
        UserReportingRelationshipId relationshipId,
        UserId? performedByUserId = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserReportingRelationship>> GetUserRelationshipsAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<SuperiorDto>> GetSuperiorsByTypeAsync(
        TenantId tenantId,
        UserId userId,
        RelationshipType type,
        CancellationToken ct = default);

    Task<IReadOnlyList<SuperiorDto>> GetDirectSuperiorsAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<SuperiorDto?> GetPrimarySuperiorAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<SubordinateDto>> GetDirectSubordinatesAsync(
        TenantId tenantId,
        UserId userId,
        RelationshipType? filterType = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<SubordinateDto>> GetAllSubordinatesAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<SuperiorDto>> GetCommandChainAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<bool> CanCommandAsync(
        TenantId tenantId,
        UserId commanderId,
        UserId targetId,
        CancellationToken ct = default);

    Task<bool> CanCommandByTypeAsync(
        TenantId tenantId,
        UserId commanderId,
        UserId targetId,
        RelationshipType type,
        CancellationToken ct = default);

    Task<OrganizationTreeNodeDto?> GetOrganizationTreeAsync(
        TenantId tenantId,
        CancellationToken ct = default);

    Task<OrganizationTreeNodeDto?> GetFullOrganizationTreeAsync(
        TenantId tenantId,
        CancellationToken ct = default);

    Task<int> GetHierarchyLevelAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task SyncPrimaryReportsToAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<HierarchyRelationshipDto>> GetUserRelationshipsDtoAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken ct = default);
}
