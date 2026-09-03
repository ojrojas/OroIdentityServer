using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using OroIdentityServer.Shared.Options;

namespace IdentityServer.Client.Services;

public class AdminBrandingService(HttpClient client) : IAdminBrandingService
{
    public Task<BrandingOptions?> GetBrandingOptionsAsync(PagedRequest? request = null, CancellationToken ct = default)  
        => client.GetFromJsonAsync<BrandingOptions>("/api/branding", ClientJsonOptions.Default, ct);
}