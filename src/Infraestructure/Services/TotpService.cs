// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Services;

using OtpNet;
using System.Security.Cryptography;

/// <summary>
/// RFC 6238 TOTP over RFC 4648 base32 secrets, which is what Google Authenticator,
/// Microsoft Authenticator, 1Password and friends implement.
/// </summary>
public sealed class TotpService : ITotpService
{
    /// <summary>Authenticator apps standardise on 30 second steps.</summary>
    private const int StepSeconds = 30;

    private const int TotpCodeDigits = 6;

    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>One step of tolerance each way, for phones whose clock drifts.</summary>
    private static readonly VerificationWindow Window = new(previous: 1, future: 1);

    public string GenerateSecret()
    {
        // 20 bytes = 160 bits, the size RFC 4226 recommends for HOTP.
        var bytes = RandomNumberGenerator.GetBytes(20);
        return Base32Encoding.ToString(bytes);
    }

    /// <summary>
    /// Builds the Key URI Format (otpauth://totp) that the QR code encodes. Written out here
    /// rather than delegated so the parameters the authenticator receives are explicit and
    /// testable: every mainstream app reads secret/issuer/algorithm/digits/period.
    /// </summary>
    public string GetProvisioningUri(string issuer, string accountLabel, string base32Secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountLabel);
        ArgumentException.ThrowIfNullOrWhiteSpace(base32Secret);

        var label = $"{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountLabel)}";
        var parameters = new Dictionary<string, string>
        {
            ["secret"] = base32Secret,
            ["issuer"] = issuer,
            ["algorithm"] = "SHA1",
            ["digits"] = TotpCodeDigits.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["period"] = StepSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        var query = string.Join("&", parameters.Select(p =>
            $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

        return $"otpauth://totp/{label}?{query}";
    }

    public bool TryVerifyCode(string base32Secret, string code, DateTime utcNow, out long timeStep)
    {
        var currentStep = GetTimeStep(utcNow);
        timeStep = currentStep;

        if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code))
            return false;

        var normalized = NormalizeCode(code);
        if (normalized.Length != TotpCodeDigits)
            return false;

        try
        {
            var totp = new Totp(Base32Encoding.ToBytes(base32Secret));

            // The timestamp overload is used on purpose: the plain VerifyTotp(code, ...) reads the
            // machine clock, which would ignore the instant the caller passed and make the
            // verification impossible to test.
            if (!totp.VerifyTotp(ToUtc(utcNow), normalized, out var matchedStep, Window))
                return false;

            // The matched step is what the caller has to remember, not the current one: with a
            // +/-1 window a code can be accepted for a step the caller is not in yet, and
            // recording the current step would leave that code replayable.
            timeStep = matchedStep;
            return true;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            // Malformed secret in the database: fail closed instead of throwing on the login path.
            return false;
        }
    }

    public long GetTimeStep(DateTime utcNow)
        => (long)(ToUtc(utcNow) - UnixEpoch).TotalSeconds / StepSeconds;

    /// <summary>
    /// Timestamps without a kind are treated as UTC rather than as local: they come from the
    /// database or from a caller that only has a wall-clock value, and silently shifting them by
    /// the server's offset would move the accepted code windows.
    /// </summary>
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string NormalizeCode(string code)
    {
        Span<char> buffer = stackalloc char[TotpCodeDigits];
        var length = 0;

        foreach (var ch in code)
        {
            if (!char.IsDigit(ch)) continue;
            if (length == TotpCodeDigits) return string.Empty; // more than six digits: reject outright
            buffer[length++] = ch;
        }

        return length == TotpCodeDigits ? new string(buffer) : string.Empty;
    }
}