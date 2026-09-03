using System.Net.Http.Json;
using System.Text.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;

namespace IdentityServer.Client.Services;

public class AdminRoleService(HttpClient client) : IAdminRoleService
{
    public async Task<ApiResponse<PagedResponse<RoleModel>>?> GetRolesAsync(PagedRequest? request = null, CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Query, "api/roles")
        {
            Content = JsonContent.Create(request ?? new PagedRequest())
        };
        var response = await client.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ApiResponse<PagedResponse<RoleModel>>>(content, ClientJsonOptions.Default);
    }

    public Task<ApiResponse<RoleModel>?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<RoleModel>>($"api/roles/{id}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/roles", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/roles/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteRoleAsync(Guid id, CancellationToken ct = default)
        => client.DeleteAsync($"api/roles/{id}", ct);

    public Task<HttpResponseMessage> ActivateRoleAsync(Guid id, CancellationToken ct = default)
        => client.PostAsync($"api/roles/{id}/activate", null, ct);
}
