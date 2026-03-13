using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class SessionsService(
    ILogger<SessionsService> logger,
    HttpClient httpClient,
    IUsersService usersService) : ISessionsService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string ROUTE = "sessions";
    private readonly HttpClient _httpClient = httpClient;
    private readonly IUsersService _usersService = usersService;

    public async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken cancellationToken)
    {
        var allUsers = await _usersService.GetAllUsersAsync(cancellationToken);
        var sessions = new List<SessionViewModel>();

        if (allUsers?.Data != null)
        {
            // build a lookup to enrich session display names
            var userLookup = allUsers.Data.Where(u => u?.Id != null).ToDictionary(u => u!.Id!.Value, u => u!);

            foreach (var u in allUsers.Data)
            {
                if (u == null) continue;
                try
                {
                    using var response = await _httpClient.GetAsync($"{ROUTE}/users/{u.Id.Value}", cancellationToken);
                    if (!response.IsSuccessStatusCode) continue;
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    var resp = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<SessionViewModel>>>(json, options);
                    if (resp?.Data != null)
                    {
                        foreach (var s in resp.Data)
                        {
                            if (s == null) continue;
                            if (s.UserId?.Value != null && userLookup.TryGetValue(s.UserId.Value, out var user))
                            {
                                s.UserDisplayName = $"{user.Name} {user.LastName} ({user.UserName})";
                            }
                            sessions.Add(s);
                        }
                    }
                }
                catch
                {
                    // ignore per-user failures
                }
            }
        }

        return new BaseResponseViewModel<IEnumerable<SessionViewModel>> { Data = sessions };
    }

    public async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"{ROUTE}/users/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<SessionViewModel>>>(json, options)
                     ?? new BaseResponseViewModel<IEnumerable<SessionViewModel>> { Data = Array.Empty<SessionViewModel>() };
        return result;
    }

    public async Task TerminateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"{ROUTE}/{sessionId}", cancellationToken);
    }
}
