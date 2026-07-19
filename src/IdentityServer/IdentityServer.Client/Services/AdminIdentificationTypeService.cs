using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.IdentificationTypes;

namespace IdentityServer.Client.Services;

public class AdminIdentificationTypeService(HttpClient client) : IAdminIdentificationTypeService
{
    public Task<ApiResponse<IEnumerable<IdentificationTypeModel>>?> GetIdentificationTypesAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IEnumerable<IdentificationTypeModel>>>("api/identification-types", ClientJsonOptions.Default, ct);

    public Task<ApiResponse<IdentificationTypeModel>?> GetIdentificationTypeByIdAsync(Guid id, CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<IdentificationTypeModel>>($"api/identification-types/{id}", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> CreateIdentificationTypeAsync(CreateIdentificationTypeRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/identification-types", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> UpdateIdentificationTypeAsync(Guid id, UpdateIdentificationTypeRequest request, CancellationToken ct = default)
        => client.PutAsJsonAsync($"api/identification-types/{id}", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DeleteIdentificationTypeAsync(Guid id, CancellationToken ct = default)
        => client.DeleteAsync($"api/identification-types/{id}", ct);
}
