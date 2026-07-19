using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Permissions;

namespace IdentityServer.Client.Interfaces;

public interface IAdminPermissionService
{
    Task<ApiResponse<IEnumerable<PermissionModel>>?> GetPermissionsAsync(CancellationToken ct = default);
    Task<ApiResponse<PermissionModel>?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeletePermissionAsync(Guid id, CancellationToken ct = default);
}
