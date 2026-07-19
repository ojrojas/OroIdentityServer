using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Permissions;

namespace IdentityServer.Client.Services;

public class AdminPermissionService(HttpClient client) : IAdminPermissionService
{
    public Task<ApiResponse<IEnumerable<PermissionModel>>?> GetPermissionsAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<PermissionModel>>>("api/permissions", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<PermissionModel>?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<PermissionModel>>($"api/permissions/{id}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/permissions", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/permissions/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeletePermissionAsync(Guid id, CancellationToken ct = default)
        => client.DeleteAsync($"api/permissions/{id}", ct);
}
