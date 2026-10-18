// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Hierarchy.Queries;

using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;

/// <summary>
/// The hierarchy claim payload for one user in one tenant: the effective level plus the
/// reporting relationships the <c>CanManageHierarchy</c> / <c>CanViewSubordinates</c> policies
/// and the <c>hierarchy_level</c> / <c>direct_superior_ids</c> claims are built from.
/// </summary>
public record GetUserHierarchyClaimsQuery(Guid TenantId, Guid UserId) : IQuery<GetUserHierarchyClaimsResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public sealed record UserHierarchyClaimsDto(
    int HierarchyLevel,
    IReadOnlyList<string> DirectSuperiorIds,
    string? PrimarySuperiorId,
    IReadOnlyList<string> RelationshipTypes);

public sealed record GetUserHierarchyClaimsResponse : BaseResponse<UserHierarchyClaimsDto>;

/// <summary>
/// Reads through <see cref="IHierarchyService"/>, never the DbContext: the level resolution
/// (tenant membership first, role fallback) and the active-relationship filter live in exactly
/// one place, shared by the admin UI and the sign-in principal.
/// </summary>
public sealed class GetUserHierarchyClaimsQueryHandler(
    ILogger<GetUserHierarchyClaimsQueryHandler> logger,
    IHierarchyService hierarchyService) : IQueryHandler<GetUserHierarchyClaimsQuery, GetUserHierarchyClaimsResponse>
{
    public async Task<GetUserHierarchyClaimsResponse> HandleAsync(
        GetUserHierarchyClaimsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling GetUserHierarchyClaimsQuery for user {UserId} in tenant {TenantId}",
            query.UserId, query.TenantId);

        var tenantId = new TenantId(query.TenantId);
        var userId = new UserId(query.UserId);

        var level = await hierarchyService.GetHierarchyLevelAsync(tenantId, userId, cancellationToken);
        var relationships = await hierarchyService.GetUserRelationshipsAsync(tenantId, userId, cancellationToken);

        var superiorIds = relationships
            .Select(r => r.ReportsToUserId.Value.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var primary = relationships
            .FirstOrDefault(r => r.RelationshipType == RelationshipType.Functional && r.Priority == 1);

        var types = relationships
            .Select(r => r.RelationshipType.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new GetUserHierarchyClaimsResponse
        {
            Data = new UserHierarchyClaimsDto(
                level,
                superiorIds,
                primary?.ReportsToUserId.Value.ToString(),
                types),
            Message = "Hierarchy claims retrieved successfully."
        };
    }
}
