using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;

namespace IdentityServer.Client.Interfaces;

public interface IAdminRoleService
{
    Task<ApiResponse<IEnumerable<RoleModel>>?> GetRolesAsync(CancellationToken ct = default);
    Task<ApiResponse<RoleModel>?> GetRoleByIdAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteRoleAsync(Guid id, CancellationToken ct = default);
}
