using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Tenants;

namespace IdentityServer.Client.Interfaces;

public interface IAdminTenantService
{
    Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsAsync(CancellationToken ct = default);
    Task<ApiResponse<TenantDetailModel>?> GetTenantByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> ActivateTenantAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> SuspendTenantAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> AddTenantUserAsync(Guid id, AddTenantUserRequest request, CancellationToken ct = default);
}
