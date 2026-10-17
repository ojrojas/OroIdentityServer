namespace IdentityServer.Client.Models.Mfa;

public sealed record MfaStatusModel(
    bool IsEnabled,
    bool HasPendingEnrollment,
    bool TenantRequiresMfa,
    int UnusedRecoveryCodes,
    DateTime? ConfirmedAtUtc,
    DateTime? LastVerifiedAtUtc);

/// <summary>
/// The enrolment payload. The QR arrives as a data URI so the page can render it without a second
/// round trip (the wizard needs the secret and the image at the same time).
/// </summary>
public sealed record MfaEnrollmentModel(
    string ProvisioningUri,
    string ManualKey,
    string QrCodeDataUri);

public sealed record MfaConfirmRequest(string Code);

public sealed record MfaDisableRequest(string Password, string Code);

public sealed record RecoveryCodesModel(IReadOnlyList<string> RecoveryCodes);