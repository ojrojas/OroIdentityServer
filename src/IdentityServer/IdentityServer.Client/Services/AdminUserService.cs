using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;

namespace IdentityServer.Client.Services;

public class AdminUserService(HttpClient client) : IAdminUserService
{
    public Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<UserModel>>>("api/users", ClientJsonOptions.Default, ct);

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
}
