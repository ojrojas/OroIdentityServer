using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;

namespace IdentityServer.Client.Interfaces;

public interface IAdminUserService
{
    Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteUserAsync(Guid id, CancellationToken ct = default);
}
