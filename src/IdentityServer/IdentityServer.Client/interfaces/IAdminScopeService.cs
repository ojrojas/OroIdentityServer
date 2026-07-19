using IdentityServer.Client.Models.OpenIddict;

namespace IdentityServer.Client.Interfaces;

public interface IAdminScopeService
{
    Task<IEnumerable<OpenIddictScopeModel>?> GetScopesAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateScopeAsync(CreateOpenIddictScopeRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateScopeAsync(string name, UpdateOpenIddictScopeRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteScopeAsync(string name, CancellationToken ct = default);
}
