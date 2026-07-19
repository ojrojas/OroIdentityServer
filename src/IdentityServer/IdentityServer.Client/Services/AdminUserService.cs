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
}
