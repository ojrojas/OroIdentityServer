using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using Microsoft.Extensions.Options;
using OroIdentityServer.Shared.Options;

namespace IdentityServer.Services;

public class ServerBrandingService(IOptions<BrandingOptions> branding) : IAdminBrandingService
{
    public async Task<BrandingOptions?> GetBrandingOptionsAsync(PagedRequest? request = null, CancellationToken ct = default)
    {
        return await Task.FromResult(branding.Value);
    }
}