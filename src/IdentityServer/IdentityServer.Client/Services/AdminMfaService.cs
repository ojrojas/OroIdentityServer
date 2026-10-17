using System.Net.Http.Json;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Mfa;

namespace IdentityServer.Client.Services;

public class AdminMfaService(HttpClient client) : IAdminMfaService
{
    public Task<ApiResponse<MfaStatusModel>?> GetStatusAsync(CancellationToken ct = default)
        => client.GetFromJsonAsync<ApiResponse<MfaStatusModel>>("api/me/mfa", ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> StartEnrollmentAsync(CancellationToken ct = default)
        => client.PostAsync("api/me/mfa/setup", null, ct);

    public Task<HttpResponseMessage> ConfirmEnrollmentAsync(string code, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/me/mfa/confirm", new MfaConfirmRequest(code), ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> DisableAsync(MfaDisableRequest request, CancellationToken ct = default)
        => client.PostAsJsonAsync("api/me/mfa/disable", request, ClientJsonOptions.Default, ct);

    public Task<HttpResponseMessage> RegenerateRecoveryCodesAsync(CancellationToken ct = default)
        => client.PostAsync("api/me/mfa/recovery-codes", null, ct);
}