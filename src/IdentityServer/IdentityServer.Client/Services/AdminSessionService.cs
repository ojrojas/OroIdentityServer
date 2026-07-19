using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Sessions;

namespace IdentityServer.Client.Services;

public class AdminSessionService(HttpClient client) : IAdminSessionService
{
    public Task<ApiResponse<IEnumerable<SessionModel>>?> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<SessionModel>>>($"api/sessions/by-user/{userId}", ClientJsonOptions.Default, ct);
}
