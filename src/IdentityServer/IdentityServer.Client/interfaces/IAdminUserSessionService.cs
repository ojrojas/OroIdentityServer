using IdentityServer.Client.Models.UserSessions;

namespace IdentityServer.Client.Interfaces;

public interface IAdminUserSessionService
{
    Task<IEnumerable<UserSessionModel>?> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateUserSessionAsync(CreateUserSessionRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeactivateUserSessionAsync(Guid id, CancellationToken ct = default);
}
