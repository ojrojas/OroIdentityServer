// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.Hierarchy.DTOs;
using OroIdentityServer.Application.Shared;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Shared;

namespace OroIdentityServer.Application.Modules.Hierarchy.Queries;

public record GetRelationshipsQuery(Guid TenantId, Guid UserId) : IQuery<GetRelationshipsResponse>;
public record GetRelationshipsResponse : BaseResponse<IEnumerable<HierarchyRelationshipDto>>;
public class GetRelationshipsQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetRelationshipsQuery, GetRelationshipsResponse>
{
    public async Task<GetRelationshipsResponse> HandleAsync(GetRelationshipsQuery q, CancellationToken ct)
    {
        var dtos = await hierarchyService.GetUserRelationshipsDtoAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetRelationshipsResponse { Data = dtos.Select(d => new HierarchyRelationshipDto(d.Id, d.TenantId, d.UserId, d.ReportsToUserId, d.RelationshipType.ToString(), d.Priority, d.IsActive, d.CreatedAtUtc, d.UpdatedAtUtc)) };
    }
}

public record GetDirectSuperiorsQuery(Guid TenantId, Guid UserId) : IQuery<GetDirectSuperiorsResponse>;
public record GetDirectSuperiorsResponse : BaseResponse<IEnumerable<SuperiorDto>>;
public class GetDirectSuperiorsQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetDirectSuperiorsQuery, GetDirectSuperiorsResponse>
{
    public async Task<GetDirectSuperiorsResponse> HandleAsync(GetDirectSuperiorsQuery q, CancellationToken ct)
    {
        var r = await hierarchyService.GetDirectSuperiorsAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetDirectSuperiorsResponse { Data = r.Select(s => new SuperiorDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName)) };
    }
}

public record GetPrimarySuperiorQuery(Guid TenantId, Guid UserId) : IQuery<GetPrimarySuperiorResponse>;
public record GetPrimarySuperiorResponse : BaseResponse<SuperiorDto>;
public class GetPrimarySuperiorQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetPrimarySuperiorQuery, GetPrimarySuperiorResponse>
{
    public async Task<GetPrimarySuperiorResponse> HandleAsync(GetPrimarySuperiorQuery q, CancellationToken ct)
    {
        var s = await hierarchyService.GetPrimarySuperiorAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetPrimarySuperiorResponse { Data = s == null ? null : new SuperiorDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName) };
    }
}

public record GetSuperiorsByTypeQuery(Guid TenantId, Guid UserId, string Type) : IQuery<GetSuperiorsByTypeResponse>;
public record GetSuperiorsByTypeResponse : BaseResponse<IEnumerable<SuperiorDto>>;
public class GetSuperiorsByTypeQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetSuperiorsByTypeQuery, GetSuperiorsByTypeResponse>
{
    public async Task<GetSuperiorsByTypeResponse> HandleAsync(GetSuperiorsByTypeQuery q, CancellationToken ct)
    {
        if (!Enum.TryParse<RelationshipType>(q.Type, true, out var t)) return new GetSuperiorsByTypeResponse { StatusCode = 400, Message = $"Invalid type {q.Type}", Errors = [$"Invalid type {q.Type}"] };
        var r = await hierarchyService.GetSuperiorsByTypeAsync(new TenantId(q.TenantId), new UserId(q.UserId), t, ct);
        return new GetSuperiorsByTypeResponse { Data = r.Select(s => new SuperiorDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName)) };
    }
}

public record GetDirectSubordinatesQuery(Guid TenantId, Guid UserId, string? Type) : IQuery<GetDirectSubordinatesResponse>;
public record GetDirectSubordinatesResponse : BaseResponse<IEnumerable<SubordinateDto>>;
public class GetDirectSubordinatesQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetDirectSubordinatesQuery, GetDirectSubordinatesResponse>
{
    public async Task<GetDirectSubordinatesResponse> HandleAsync(GetDirectSubordinatesQuery q, CancellationToken ct)
    {
        RelationshipType? filter = null;
        if (!string.IsNullOrWhiteSpace(q.Type) && Enum.TryParse<RelationshipType>(q.Type, true, out var p)) filter = p;
        else if (!string.IsNullOrWhiteSpace(q.Type)) return new GetDirectSubordinatesResponse { StatusCode = 400, Message = $"Invalid type {q.Type}", Errors = [$"Invalid type {q.Type}"] };
        var r = await hierarchyService.GetDirectSubordinatesAsync(new TenantId(q.TenantId), new UserId(q.UserId), filter, ct);
        return new GetDirectSubordinatesResponse { Data = r.Select(s => new SubordinateDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName)) };
    }
}

public record GetAllSubordinatesQuery(Guid TenantId, Guid UserId) : IQuery<GetAllSubordinatesResponse>;
public record GetAllSubordinatesResponse : BaseResponse<IEnumerable<SubordinateDto>>;
public class GetAllSubordinatesQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetAllSubordinatesQuery, GetAllSubordinatesResponse>
{
    public async Task<GetAllSubordinatesResponse> HandleAsync(GetAllSubordinatesQuery q, CancellationToken ct)
    {
        var r = await hierarchyService.GetAllSubordinatesAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetAllSubordinatesResponse { Data = r.Select(s => new SubordinateDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName)) };
    }
}

public record GetCommandChainQuery(Guid TenantId, Guid UserId) : IQuery<GetCommandChainResponse>;
public record GetCommandChainResponse : BaseResponse<IEnumerable<SuperiorDto>>;
public class GetCommandChainQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetCommandChainQuery, GetCommandChainResponse>
{
    public async Task<GetCommandChainResponse> HandleAsync(GetCommandChainQuery q, CancellationToken ct)
    {
        var r = await hierarchyService.GetCommandChainAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetCommandChainResponse { Data = r.Select(s => new SuperiorDto(s.UserId, s.UserName, s.Email, s.RelationshipType.ToString(), s.Priority, s.HierarchyLevel, s.RoleName)) };
    }
}

public record GetOrganizationTreeQuery(Guid TenantId) : IQuery<GetOrganizationTreeResponse>;
public record GetOrganizationTreeResponse : BaseResponse<OrganizationTreeNodeDto?>;
public class GetOrganizationTreeQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetOrganizationTreeQuery, GetOrganizationTreeResponse>
{
    public async Task<GetOrganizationTreeResponse> HandleAsync(GetOrganizationTreeQuery q, CancellationToken ct)
    {
        var n = await hierarchyService.GetOrganizationTreeAsync(new TenantId(q.TenantId), ct);
        return new GetOrganizationTreeResponse { Data = n == null ? null : Map(n) };
    }
    private static OrganizationTreeNodeDto Map(OroIdentityServer.Core.Modules.Hierarchy.DTOs.OrganizationTreeNodeDto dto) =>
        new(dto.UserId, dto.UserName, dto.Email, dto.RoleName, dto.HierarchyLevel, dto.Children.Select(Map).ToList(), dto.SecondaryRelationships.Select(r => new HierarchyRelationshipDto(r.Id, r.TenantId, r.UserId, r.ReportsToUserId, r.RelationshipType.ToString(), r.Priority, r.IsActive, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList());
}

public record GetFullOrganizationTreeQuery(Guid TenantId) : IQuery<GetFullOrganizationTreeResponse>;
public record GetFullOrganizationTreeResponse : BaseResponse<OrganizationTreeNodeDto?>;
public class GetFullOrganizationTreeQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetFullOrganizationTreeQuery, GetFullOrganizationTreeResponse>
{
    public async Task<GetFullOrganizationTreeResponse> HandleAsync(GetFullOrganizationTreeQuery q, CancellationToken ct)
    {
        var n = await hierarchyService.GetFullOrganizationTreeAsync(new TenantId(q.TenantId), ct);
        return new GetFullOrganizationTreeResponse { Data = n == null ? null : Map(n) };
    }
    private static OrganizationTreeNodeDto Map(OroIdentityServer.Core.Modules.Hierarchy.DTOs.OrganizationTreeNodeDto dto) =>
        new(dto.UserId, dto.UserName, dto.Email, dto.RoleName, dto.HierarchyLevel, dto.Children.Select(Map).ToList(), dto.SecondaryRelationships.Select(r => new HierarchyRelationshipDto(r.Id, r.TenantId, r.UserId, r.ReportsToUserId, r.RelationshipType.ToString(), r.Priority, r.IsActive, r.CreatedAtUtc, r.UpdatedAtUtc)).ToList());
}

public record CanCommandQuery(Guid TenantId, Guid CommanderId, Guid TargetId, string? Type) : IQuery<CanCommandResponse>;
public record CanCommandResponse : BaseResponse<bool>;
public class CanCommandQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<CanCommandQuery, CanCommandResponse>
{
    public async Task<CanCommandResponse> HandleAsync(CanCommandQuery q, CancellationToken ct)
    {
        bool can;
        if (!string.IsNullOrWhiteSpace(q.Type) && Enum.TryParse<RelationshipType>(q.Type, true, out var t))
            can = await hierarchyService.CanCommandByTypeAsync(new TenantId(q.TenantId), new UserId(q.CommanderId), new UserId(q.TargetId), t, ct);
        else if (!string.IsNullOrWhiteSpace(q.Type))
            return new CanCommandResponse { StatusCode = 400, Message = $"Invalid type {q.Type}", Errors = [$"Invalid type {q.Type}"] };
        else
            can = await hierarchyService.CanCommandAsync(new TenantId(q.TenantId), new UserId(q.CommanderId), new UserId(q.TargetId), ct);
        return new CanCommandResponse { Data = can };
    }
}

public record GetHierarchyLevelQuery(Guid TenantId, Guid UserId) : IQuery<GetHierarchyLevelResponse>;
public record GetHierarchyLevelResponse : BaseResponse<int>;
public class GetHierarchyLevelQueryHandler(IHierarchyService hierarchyService) : IQueryHandler<GetHierarchyLevelQuery, GetHierarchyLevelResponse>
{
    public async Task<GetHierarchyLevelResponse> HandleAsync(GetHierarchyLevelQuery q, CancellationToken ct)
    {
        var l = await hierarchyService.GetHierarchyLevelAsync(new TenantId(q.TenantId), new UserId(q.UserId), ct);
        return new GetHierarchyLevelResponse { Data = l };
    }
}
