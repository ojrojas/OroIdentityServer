using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.OpenIddict;

namespace IdentityServer.Client.Services;

public class AdminScopeService(HttpClient client) : IAdminScopeService
{
    public Task<IEnumerable<OpenIddictScopeModel>?> GetScopesAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<IEnumerable<OpenIddictScopeModel>>("api/scopes", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateScopeAsync(CreateOpenIddictScopeRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/scopes", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateScopeAsync(string name, UpdateOpenIddictScopeRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/scopes/{name}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteScopeAsync(string name, CancellationToken ct = default)
        => client.DeleteAsync($"api/scopes/{name}", ct);
}
