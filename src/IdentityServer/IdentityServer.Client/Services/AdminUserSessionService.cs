using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.UserSessions;

namespace IdentityServer.Client.Services;

public class AdminUserSessionService(HttpClient client) : IAdminUserSessionService
{
    public Task<IEnumerable<UserSessionModel>?> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => client.GetFromJsonAsync<IEnumerable<UserSessionModel>>($"api/user-sessions/by-user/{userId}", ClientJsonOptions.Default, ct);

    public Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<int>("api/user-sessions/active-count", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateUserSessionAsync(CreateUserSessionRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/user-sessions", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeactivateUserSessionAsync(Guid id, CancellationToken ct = default)
        => client.PostAsync($"api/user-sessions/{id}/deactivate", null, ct);

    public Task<HttpResponseMessage> TerminateAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
        => client.PostAsync($"api/user-sessions/terminate-all/{userId}", null, ct);
}
