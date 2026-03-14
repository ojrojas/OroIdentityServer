using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface ITenantsService
{
    Task<BaseResponseViewModel<IEnumerable<TenantViewModel>>> GetAllTenantsAsync(CancellationToken cancellationToken);
    Task<TenantViewModel?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken);
    Task CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken);
    Task UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken);
    Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken);
}
