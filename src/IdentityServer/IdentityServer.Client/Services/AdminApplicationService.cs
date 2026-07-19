using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.OpenIddict;

namespace IdentityServer.Client.Services;

public class AdminApplicationService(HttpClient client) : IAdminApplicationService
{
    public Task<IEnumerable<OpenIddictApplicationModel>?> GetApplicationsAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<IEnumerable<OpenIddictApplicationModel>>("api/applications", ClientJsonOptions.Default, ct);

    public Task<OpenIddictApplicationModel?> GetApplicationByClientIdAsync(string clientId, CancellationToken ct = default)
        => client.GetFromJsonAsync<OpenIddictApplicationModel>($"api/applications/{clientId}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateApplicationAsync(OpenIddictApplicationModel application, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/applications", application, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateApplicationAsync(string clientId, OpenIddictApplicationModel application, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/applications/{clientId}", application, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteApplicationAsync(string clientId, CancellationToken ct = default)
        => client.DeleteAsync($"api/applications/{clientId}", ct);
}
