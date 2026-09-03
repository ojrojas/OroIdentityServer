using IdentityServer.Client.Models;
using IdentityServer.Client.Models.OpenIddict;

namespace IdentityServer.Client.Interfaces;

public interface IAdminApplicationService
{
    Task<PagedResponse<OpenIddictApplicationModel>?> GetApplicationsAsync(PagedRequest? request = null, CancellationToken ct = default);
    Task<OpenIddictApplicationModel?> GetApplicationByClientIdAsync(string clientId, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateApplicationAsync(OpenIddictApplicationModel application, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateApplicationAsync(string clientId, OpenIddictApplicationModel application, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteApplicationAsync(string clientId, CancellationToken ct = default);
}
