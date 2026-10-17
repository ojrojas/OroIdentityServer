using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Mfa;

namespace IdentityServer.Client.Interfaces;

/// <summary>
/// Self-service MFA management. Implemented twice: over HTTP by the WASM client and over CQRS by
/// the server host, so a page rendered under InteractiveAuto can talk to either side.
/// </summary>
public interface IAdminMfaService
{
    Task<ApiResponse<MfaStatusModel>?> GetStatusAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> StartEnrollmentAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> ConfirmEnrollmentAsync(string code, CancellationToken ct = default);
    Task<HttpResponseMessage> DisableAsync(MfaDisableRequest request, CancellationToken ct = default);
    Task<HttpResponseMessage> RegenerateRecoveryCodesAsync(CancellationToken ct = default);
}