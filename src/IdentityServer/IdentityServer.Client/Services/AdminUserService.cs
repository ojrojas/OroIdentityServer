using System.Net.Http.Json;
using System.Text.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;

namespace IdentityServer.Client.Services;

public class AdminUserService(HttpClient client) : IAdminUserService
{
    public Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<UserModel>>>("api/users", ClientJsonOptions.Default, ct);

    public async Task<ApiResponse<PagedResponse<UserModel>>?> GetUsersPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Query, "api/users/paged")
        {
            Content = JsonContent.Create(request)
        };
        var response = await client.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ApiResponse<PagedResponse<UserModel>>>(content, ClientJsonOptions.Default);
    }

    public Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/users", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/users/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteUserAsync(Guid id, CancellationToken ct = default)
        => client.DeleteAsync($"api/users/{id}", ct);

    public Task<HttpResponseMessage> AssignRolesToUserAsync(Guid userId, AssignRolesRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/users/{userId}/roles", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> LockUserAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/users/{userId}/lock", null, ct);

    public Task<HttpResponseMessage> UnlockUserAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/users/{userId}/unlock", null, ct);

    public Task<HttpResponseMessage> DeactivateUserAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/users/{userId}/deactivate", null, ct);

    public Task<HttpResponseMessage> ActivateUserAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/users/{userId}/activate", null, ct);

    public Task<ApiResponse<UserModel>?> GetUserByIdAsync(Guid Id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<UserModel>>($"api/users/{Id}", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersByRoleAndTenantAsync(string role, Guid? tenantId = null, CancellationToken ct = default)
    {
        var url = $"api/users/by-role/{Uri.EscapeDataString(role)}";
        if (tenantId.HasValue)
            url += $"?tenantId={tenantId.Value}";
        return client.GetFromJsonAsync<ApiResponse<IEnumerable<UserModel>>>(url, ClientJsonOptions.Default, ct);
    }
}
