using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Hierarchy;

namespace IdentityServer.Client.Services;

public class AdminHierarchyService(HttpClient client) : IAdminHierarchyService
{
    public Task<HttpResponseMessage> CreateRelationshipAsync(CreateRelationshipRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/hierarchy/relationships", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdatePriorityAsync(Guid relationshipId, UpdatePriorityRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/hierarchy/relationships/{relationshipId}/priority", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteRelationshipAsync(Guid relationshipId, CancellationToken ct = default)
        => client.DeleteAsync($"api/hierarchy/relationships/{relationshipId}", ct);

    public Task<IEnumerable<HierarchyRelationshipModel>?> GetRelationshipsAsync(Guid userId, CancellationToken ct = default)
        => client.GetFromJsonAsync<IEnumerable<HierarchyRelationshipModel>>($"api/hierarchy/relationships/{userId}", ClientJsonOptions.Default, ct);

    public Task<IEnumerable<SuperiorModel>?> GetSuperiorsAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/superiors/{userId.Value}" : "api/hierarchy/superiors";
        return client.GetFromJsonAsync<IEnumerable<SuperiorModel>>(url, ClientJsonOptions.Default, ct);
    }

    public Task<SuperiorModel?> GetPrimarySuperiorAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/superiors/{userId.Value}/primary" : "api/hierarchy/superiors/primary";
        return client.GetFromJsonAsync<SuperiorModel>(url, ClientJsonOptions.Default, ct);
    }

    public Task<IEnumerable<SuperiorModel>?> GetSuperiorsByTypeAsync(Guid? userId, string type, CancellationToken ct = default)
    {
        var baseUrl = userId.HasValue ? $"api/hierarchy/superiors/{userId.Value}/by-type/{type}" : $"api/hierarchy/superiors/by-type/{type}";
        return client.GetFromJsonAsync<IEnumerable<SuperiorModel>>(baseUrl, ClientJsonOptions.Default, ct);
    }

    public Task<IEnumerable<SubordinateModel>?> GetSubordinatesAsync(Guid? userId = null, string? type = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/subordinates/{userId.Value}" : "api/hierarchy/subordinates";
        if (!string.IsNullOrWhiteSpace(type)) url += $"?type={Uri.EscapeDataString(type)}";
        return client.GetFromJsonAsync<IEnumerable<SubordinateModel>>(url, ClientJsonOptions.Default, ct);
    }

    public Task<IEnumerable<SubordinateModel>?> GetAllSubordinatesAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/subordinates/{userId.Value}/all" : "api/hierarchy/subordinates/all";
        return client.GetFromJsonAsync<IEnumerable<SubordinateModel>>(url, ClientJsonOptions.Default, ct);
    }

    public Task<IEnumerable<SuperiorModel>?> GetCommandChainAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/chain/{userId.Value}" : "api/hierarchy/chain";
        return client.GetFromJsonAsync<IEnumerable<SuperiorModel>>(url, ClientJsonOptions.Default, ct);
    }

    public async Task<OrganizationTreeNodeModel?> GetTreeAsync(CancellationToken ct = default)
    {
        var response = await client.GetAsync("api/hierarchy/tree", ct);
        if (response.Content.Headers.ContentLength is 0 or null) return null;
        return await response.Content.ReadFromJsonAsync<OrganizationTreeNodeModel>(ClientJsonOptions.Default, ct);
    }

    public async Task<OrganizationTreeNodeModel?> GetFullTreeAsync(CancellationToken ct = default)
    {
        var response = await client.GetAsync("api/hierarchy/tree/full", ct);
        if (response.Content.Headers.ContentLength is 0 or null) return null;
        return await response.Content.ReadFromJsonAsync<OrganizationTreeNodeModel>(ClientJsonOptions.Default, ct);
    }

    public Task<CanCommandModel?> CanCommandAsync(Guid commanderId, Guid targetId, string? type = null, CancellationToken ct = default)
    {
        var url = $"api/hierarchy/can-command/{commanderId}/{targetId}";
        if (!string.IsNullOrWhiteSpace(type)) url += $"?type={Uri.EscapeDataString(type)}";
        return client.GetFromJsonAsync<CanCommandModel>(url, ClientJsonOptions.Default, ct);
    }

    public Task<HierarchyLevelModel?> GetLevelAsync(Guid? userId = null, CancellationToken ct = default)
    {
        var url = userId.HasValue ? $"api/hierarchy/level/{userId.Value}" : "api/hierarchy/level";
        return client.GetFromJsonAsync<HierarchyLevelModel>(url, ClientJsonOptions.Default, ct);
    }

    public Task<HttpResponseMessage> SyncPrimaryAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/hierarchy/sync-primary/{userId}", null, ct);
}
