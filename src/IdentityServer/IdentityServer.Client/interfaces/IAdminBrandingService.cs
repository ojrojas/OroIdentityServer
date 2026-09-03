using IdentityServer.Client.Models;
using OroIdentityServer.Shared.Options;

namespace IdentityServer.Client.Interfaces;

public interface IAdminBrandingService
{
    Task<BrandingOptions?> GetBrandingOptionsAsync(PagedRequest? request = null, CancellationToken ct = default);
}
