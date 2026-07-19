using IdentityServer.Client.Models;
using IdentityServer.Client.Models.IdentificationTypes;

namespace IdentityServer.Client.Interfaces;

public interface IAdminIdentificationTypeService
{
    Task<ApiResponse<IEnumerable<IdentificationTypeModel>>?> GetIdentificationTypesAsync(CancellationToken ct = default);
    Task<ApiResponse<IdentificationTypeModel>?> GetIdentificationTypeByIdAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateIdentificationTypeAsync(CreateIdentificationTypeRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateIdentificationTypeAsync(Guid id, UpdateIdentificationTypeRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteIdentificationTypeAsync(Guid id, CancellationToken ct = default);
}
