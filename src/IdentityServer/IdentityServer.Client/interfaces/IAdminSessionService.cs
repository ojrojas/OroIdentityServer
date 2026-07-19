using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Sessions;

namespace IdentityServer.Client.Interfaces;

public interface IAdminSessionService
{
    Task<ApiResponse<IEnumerable<SessionModel>>?> GetByUserAsync(Guid userId, CancellationToken ct = default);
}
