using IdentityServer.Client.Models.Hierarchy;

namespace IdentityServer.Client.Interfaces;

public interface IAdminHierarchyService
{
    Task<HttpResponseMessage> CreateRelationshipAsync(CreateRelationshipRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdatePriorityAsync(Guid relationshipId, UpdatePriorityRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteRelationshipAsync(Guid relationshipId, CancellationToken ct = default);
    Task<IEnumerable<HierarchyRelationshipModel>?> GetRelationshipsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<SuperiorModel>?> GetSuperiorsAsync(Guid? userId = null, CancellationToken ct = default);
    Task<SuperiorModel?> GetPrimarySuperiorAsync(Guid? userId = null, CancellationToken ct = default);
    Task<IEnumerable<SuperiorModel>?> GetSuperiorsByTypeAsync(Guid? userId, string type, CancellationToken ct = default);
    Task<IEnumerable<SubordinateModel>?> GetSubordinatesAsync(Guid? userId = null, string? type = null, CancellationToken ct = default);
    Task<IEnumerable<SubordinateModel>?> GetAllSubordinatesAsync(Guid? userId = null, CancellationToken ct = default);
    Task<IEnumerable<SuperiorModel>?> GetCommandChainAsync(Guid? userId = null, CancellationToken ct = default);
    Task<OrganizationTreeNodeModel?> GetTreeAsync(CancellationToken ct = default);
    Task<OrganizationTreeNodeModel?> GetFullTreeAsync(CancellationToken ct = default);
    Task<CanCommandModel?> CanCommandAsync(Guid commanderId, Guid targetId, string? type = null, CancellationToken ct = default);
    Task<HierarchyLevelModel?> GetLevelAsync(Guid? userId = null, CancellationToken ct = default);
    Task<HttpResponseMessage> SyncPrimaryAsync(Guid userId, CancellationToken ct = default);
}
