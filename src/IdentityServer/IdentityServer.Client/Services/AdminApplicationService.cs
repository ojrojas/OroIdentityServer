using System.Net.Http.Json;
using System.Text.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.OpenIddict;

namespace IdentityServer.Client.Services;

public class AdminApplicationService(HttpClient client) : IAdminApplicationService
{
    public async Task<PagedResponse<OpenIddictApplicationModel>?> GetApplicationsAsync(PagedRequest? request = null, CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Query, "api/applications")
        {
            Content = JsonContent.Create(request ?? new PagedRequest())
        };
        var response = await client.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<PagedResponse<OpenIddictApplicationModel>>(content, ClientJsonOptions.Default);
    }

    public Task<OpenIddictApplicationModel?> GetApplicationByClientIdAsync(string clientId, CancellationToken ct = default)
        => client.GetFromJsonAsync<OpenIddictApplicationModel>($"api/applications/{clientId}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateApplicationAsync(OpenIddictApplicationModel application, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/applications", application, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateApplicationAsync(string clientId, OpenIddictApplicationModel application, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/applications/{clientId}", application, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteApplicationAsync(string clientId, CancellationToken ct = default)
        => client.DeleteAsync($"api/applications/{clientId}", ct);
}
