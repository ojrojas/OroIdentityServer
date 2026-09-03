using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;

namespace IdentityServer.Client.Interfaces;

public interface IAdminUserService
{
    Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResponse<UserModel>>?> GetUsersPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<ApiResponse<UserModel>?> GetUserByIdAsync(Guid Id, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteUserAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> AssignRolesToUserAsync(Guid userId, AssignRolesRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> LockUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> UnlockUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> DeactivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> ActivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersByRoleAndTenantAsync(string role, Guid? tenantId = null, CancellationToken ct = default);
}
