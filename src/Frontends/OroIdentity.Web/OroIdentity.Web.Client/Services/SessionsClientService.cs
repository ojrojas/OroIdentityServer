using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace OroIdentity.Web.Client.Services;

public class SessionsClientService(
    ILogger<SessionsClientService> logger,
    HttpClient httpClient) : ISessionsService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string ROUTE = "api/v1/sessions";

    public async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<SessionViewModel>>>(ROUTE, cancellationToken);
        return response ?? new BaseResponseViewModel<IEnumerable<SessionViewModel>>() { Data = Array.Empty<SessionViewModel>() };
    }

    public async Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<SessionViewModel>>>($"{ROUTE}/users/{userId}", cancellationToken);
        return response ?? new BaseResponseViewModel<IEnumerable<SessionViewModel>>() { Data = Array.Empty<SessionViewModel>() };
    }

    public async Task TerminateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"{ROUTE}/{sessionId}", cancellationToken);
    }
}
