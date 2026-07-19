using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Tenants;

namespace IdentityServer.Client.Services;

public class AdminTenantService(HttpClient client) : IAdminTenantService
{
    public Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<TenantModel>>>("api/tenants", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<TenantDetailModel>?> GetTenantByIdAsync(Guid id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<TenantDetailModel>>($"api/tenants/{id}", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsByUserIdAsync(Guid userId, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<TenantModel>>>($"api/tenants/by-user/{userId}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/tenants", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/tenants/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> ActivateTenantAsync(Guid id, CancellationToken ct = default)
        => client.PostAsync($"api/tenants/{id}/activate", null, ct);

    public Task<HttpResponseMessage> SuspendTenantAsync(Guid id, CancellationToken ct = default)
        => client.PostAsync($"api/tenants/{id}/suspend", null, ct);

    public Task<HttpResponseMessage> AddTenantUserAsync(Guid id, AddTenantUserRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync($"api/tenants/{id}/users", request, ClientJsonOptions.Default, ct);
}
