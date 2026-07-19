using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;

namespace IdentityServer.Client.Services;

public class AdminRoleService(HttpClient client) : IAdminRoleService
{
    public Task<ApiResponse<IEnumerable<RoleModel>>?> GetRolesAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<RoleModel>>>("api/roles", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<RoleModel>?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<RoleModel>>($"api/roles/{id}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/roles", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/roles/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteRoleAsync(Guid id, CancellationToken ct = default)
        => client.DeleteAsync($"api/roles/{id}", ct);
}
