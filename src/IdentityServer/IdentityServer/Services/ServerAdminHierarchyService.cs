using System.Net;
using System.Security.Claims;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.Hierarchy;
using OroIdentityServer.Application.Modules.Hierarchy.Commands;
using OroIdentityServer.Application.Modules.Hierarchy.Queries;
using OroIdentityServer.Shared.Authorization;
using IdentityServer.Client.Services;

namespace IdentityServer.Services;

/// <summary>
/// Server-side (BFF) implementation of IAdminHierarchyService. Vive en IdentityServer (BFF),
/// es registrada como services.AddScoped&lt;IAdminHierarchyService, ServerAdminHierarchyService&gt;()
/// y es invocada por los minimal API endpoints (Endpoints/AdminHierarchyEndpoints.cs).
/// Internamente despacha Commands/Queries del proyecto Application (via IQueryDispatcher/ICommandDispatcher)
/// hacia los handlers que a su vez llaman a IHierarchyService (Infra). Nunca hace HttpClient.
/// </summary>
public class ServerAdminHierarchyService(
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher,
    IHttpContextAccessor httpContextAccessor,
    ICurrentTenantContext tenantContext,
    ILogger<ServerAdminHierarchyService> logger) : IAdminHierarchyService
{
    private Guid GetTenantId()
    {
        if (tenantContext.CurrentTenantId.HasValue) return tenantContext.CurrentTenantId.Value;
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var header) && Guid.TryParse(header.ToString(), out var headerId))
            return headerId;
        var claim = httpContext?.User?.FindFirstValue(AuthorizationClaimTypes.TenantId) ?? httpContext?.User?.FindFirstValue("tenant_id");
        if (Guid.TryParse(claim, out var claimId)) return claimId;
        throw new InvalidOperationException("TenantId not found in request");
    }

    private Guid? GetCurrentUserId()
    {
        var sub = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var guid)) return guid;
        return null;
    }

    public async Task<HttpResponseMessage> CreateRelationshipAsync(CreateRelationshipRequest request, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var performer = GetCurrentUserId();
        var result = await commandDispatcher.SendAsync(new CreateRelationshipCommand(tenantId, request.UserId, request.ReportsToUserId, request.Type, request.Priority, performer), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdatePriorityAsync(Guid relationshipId, UpdatePriorityRequest request, CancellationToken ct = default)
    {
        var performer = GetCurrentUserId();
        var result = await commandDispatcher.SendAsync(new UpdateRelationshipPriorityCommand(relationshipId, request.Priority, performer), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.OK);
    }

    public async Task<HttpResponseMessage> DeleteRelationshipAsync(Guid relationshipId, CancellationToken ct = default)
    {
        var performer = GetCurrentUserId();
        var result = await commandDispatcher.SendAsync(new DeleteRelationshipCommand(relationshipId, performer, null), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<IEnumerable<HierarchyRelationshipModel>?> GetRelationshipsAsync(Guid userId, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var r = await queryDispatcher.SendAsync(new GetRelationshipsQuery(tenantId, userId), ct);
        return r.Data?.Select(d => new HierarchyRelationshipModel(d.Id, d.TenantId, d.UserId, d.ReportsToUserId, d.RelationshipType, d.Priority, d.IsActive, d.CreatedAtUtc, d.UpdatedAtUtc));
    }

    public async Task<IEnumerable<SuperiorModel>?> GetSuperiorsAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetDirectSuperiorsQuery(tenantId, target), ct);
        return r.Data?.Select(s => new SuperiorModel(s.UserId, s.UserName, s.Email, s.RelationshipType, s.Priority, s.HierarchyLevel, s.RoleName));
    }

    public async Task<SuperiorModel?> GetPrimarySuperiorAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetPrimarySuperiorQuery(tenantId, target), ct);
        return r.Data == null ? null : new SuperiorModel(r.Data.UserId, r.Data.UserName, r.Data.Email, r.Data.RelationshipType, r.Data.Priority, r.Data.HierarchyLevel, r.Data.RoleName);
    }

    public async Task<IEnumerable<SuperiorModel>?> GetSuperiorsByTypeAsync(Guid? userId, string type, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetSuperiorsByTypeQuery(tenantId, target, type), ct);
        if (r.StatusCode == 400) throw new InvalidOperationException(r.Message);
        return r.Data?.Select(s => new SuperiorModel(s.UserId, s.UserName, s.Email, s.RelationshipType, s.Priority, s.HierarchyLevel, s.RoleName));
    }

    public async Task<IEnumerable<SubordinateModel>?> GetSubordinatesAsync(Guid? userId = null, string? type = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetDirectSubordinatesQuery(tenantId, target, type), ct);
        if (r.StatusCode == 400) throw new InvalidOperationException(r.Message);
        return r.Data?.Select(s => new SubordinateModel(s.UserId, s.UserName, s.Email, s.RelationshipType, s.Priority, s.HierarchyLevel, s.RoleName));
    }

    public async Task<IEnumerable<SubordinateModel>?> GetAllSubordinatesAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetAllSubordinatesQuery(tenantId, target), ct);
        return r.Data?.Select(s => new SubordinateModel(s.UserId, s.UserName, s.Email, s.RelationshipType, s.Priority, s.HierarchyLevel, s.RoleName));
    }

    public async Task<IEnumerable<SuperiorModel>?> GetCommandChainAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetCommandChainQuery(tenantId, target), ct);
        return r.Data?.Select(s => new SuperiorModel(s.UserId, s.UserName, s.Email, s.RelationshipType, s.Priority, s.HierarchyLevel, s.RoleName));
    }

    public async Task<OrganizationTreeNodeModel?> GetTreeAsync(CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var r = await queryDispatcher.SendAsync(new GetOrganizationTreeQuery(tenantId), ct);
        return r.Data == null ? null : MapTree(r.Data);
    }

    public async Task<OrganizationTreeNodeModel?> GetFullTreeAsync(CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var r = await queryDispatcher.SendAsync(new GetFullOrganizationTreeQuery(tenantId), ct);
        return r.Data == null ? null : MapTree(r.Data);
    }

    public async Task<CanCommandModel?> CanCommandAsync(Guid commanderId, Guid targetId, string? type = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var r = await queryDispatcher.SendAsync(new CanCommandQuery(tenantId, commanderId, targetId, type), ct);
        if (r.StatusCode == 400) throw new InvalidOperationException(r.Message);
        return new CanCommandModel(commanderId, targetId, r.Data);
    }

    public async Task<HierarchyLevelModel?> GetLevelAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var target = userId ?? GetCurrentUserId() ?? throw new InvalidOperationException("UserId not found");
        var r = await queryDispatcher.SendAsync(new GetHierarchyLevelQuery(tenantId, target), ct);
        return new HierarchyLevelModel(target, r.Data);
    }

    public async Task<HttpResponseMessage> SyncPrimaryAsync(Guid userId, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var result = await commandDispatcher.SendAsync(new SyncPrimaryCommand(tenantId, userId), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.OK);
    }

    private static OrganizationTreeNodeModel MapTree(OroIdentityServer.Application.Modules.Hierarchy.DTOs.OrganizationTreeNodeDto dto) =>
        new(dto.UserId, dto.UserName, dto.Email, dto.RoleName, dto.HierarchyLevel, dto.Children.Select(MapTree).ToList(), dto.SecondaryRelationships.Select(r => new HierarchyRelationshipModel(r.Id, r.TenantId, r.UserId, r.ReportsToUserId, r.RelationshipType, r.Priority, r.IsActive, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList());
}
