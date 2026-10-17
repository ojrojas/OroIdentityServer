using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Core.Interfaces;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Mfa;
using OroIdentityServer.Application.Modules.Mfa.Commands;
using OroIdentityServer.Application.Modules.Mfa.Queries;

namespace IdentityServer.Services;

/// <summary>
/// CQRS-backed implementation of the self-service MFA contract. The caller is always the
/// authenticated user (the /api/me group only requires a valid session), so the user id is taken
/// from the request rather than from the payload: there is no "enable MFA for somebody else"
/// path through this service.
/// </summary>
public class ServerAdminMfaService(
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher,
    IHttpContextAccessor httpContextAccessor,
    IQrCodeGenerator qrCodeGenerator) : IAdminMfaService
{
    public async Task<ApiResponse<MfaStatusModel>?> GetStatusAsync(CancellationToken ct = default)
    {
        var callerId = GetCallerId();
        if (callerId is null) return Unauthorized<MfaStatusModel>();

        var status = await queryDispatcher.SendAsync(new GetMfaStatusQuery(callerId.Value), ct);

        return new ApiResponse<MfaStatusModel>
        {
            Data = new MfaStatusModel(
                status.IsEnabled,
                status.HasPendingEnrollment,
                status.TenantRequiresMfa,
                status.UnusedRecoveryCodes,
                status.ConfirmedAtUtc,
                status.LastVerifiedAtUtc),
            StatusCode = (int)HttpStatusCode.OK,
            Message = "MFA status retrieved."
        };
    }

    public async Task<HttpResponseMessage> StartEnrollmentAsync(CancellationToken ct = default)
    {
        var callerId = GetCallerId();
        if (callerId is null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        MfaEnrollmentPayload payload;
        try
        {
            payload = await commandDispatcher.SendAsync(new StartMfaEnrollmentCommand(callerId.Value), ct);
        }
        catch (InvalidOperationException)
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict);
        }

        var enrollment = new MfaEnrollmentModel(
            payload.ProvisioningUri,
            payload.Secret,
            ToDataUri(qrCodeGenerator.GeneratePng(payload.ProvisioningUri)));

        // ClientJsonOptions.Default: the same casing contract the WASM client deserializes with
        // (JsonContent.Create alone would emit PascalCase and break the round trip).
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(enrollment, options: ClientJsonOptions.Default)
        };
    }

    public async Task<HttpResponseMessage> ConfirmEnrollmentAsync(string code, CancellationToken ct = default)
    {
        var callerId = GetCallerId();
        if (callerId is null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        try
        {
            // Throws InvalidOperationException when the code does not match the pending secret.
            var result = await commandDispatcher.SendAsync(new ConfirmMfaEnrollmentCommand(callerId.Value, code), ct);
            return Json(new RecoveryCodesModel(result.RecoveryCodes), HttpStatusCode.OK);
        }
        catch (InvalidOperationException)
        {
            return Json(new { error = "invalid_code" }, HttpStatusCode.BadRequest);
        }
    }

    public async Task<HttpResponseMessage> DisableAsync(MfaDisableRequest request, CancellationToken ct = default)
    {
        var callerId = GetCallerId();
        if (callerId is null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var result = await commandDispatcher.SendAsync(
            new DisableMfaCommand(callerId.Value, request.Password, request.Code), ct);

        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> RegenerateRecoveryCodesAsync(CancellationToken ct = default)
    {
        var callerId = GetCallerId();
        if (callerId is null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        try
        {
            var result = await commandDispatcher.SendAsync(
                new RegenerateMfaRecoveryCodesCommand(callerId.Value), ct);

            return Json(new RecoveryCodesModel(result.RecoveryCodes), HttpStatusCode.OK);
        }
        catch (InvalidOperationException)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }

    private Guid? GetCallerId()
    {
        var raw = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    private static string ToDataUri(byte[] png)
        => $"data:image/png;base64,{Convert.ToBase64String(png)}";

    private static HttpResponseMessage Json<T>(T payload, HttpStatusCode statusCode)
        => new(statusCode) { Content = JsonContent.Create(payload, options: ClientJsonOptions.Default) };

    private static ApiResponse<T> Unauthorized<T>()
        => new()
        {
            Data = default,
            StatusCode = (int)HttpStatusCode.Unauthorized,
            Message = "User could not be identified.",
            Errors = ["unauthorized"]
        };
}