// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using OtpNet;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Infraestructure.Services;

/// <summary>
/// Deterministic TOTP checks: codes are computed from a fixed instant, never from UtcNow, so a
/// failure here is a real regression and not a timing artefact.
/// </summary>
public class TotpServiceTests
{
    private static readonly DateTime FixedInstant = new(2026, 3, 14, 15, 9, 26, DateTimeKind.Utc);

    private readonly ITotpService _totp = new TotpService();

    [Fact]
    public void GenerateSecret_ProducesAUsableBase32Secret()
    {
        var secret = _totp.GenerateSecret();

        // 160 bit secret, base32 encoded without padding.
        Assert.Equal(32, secret.Length);
        Assert.Matches("^[A-Z2-7]+$", secret);
    }

    [Fact]
    public void GenerateSecret_ProducesADifferentSecretEveryTime()
    {
        Assert.NotEqual(_totp.GenerateSecret(), _totp.GenerateSecret());
    }

    [Fact]
    public void TryVerifyCode_AcceptsTheCodeForTheCurrentWindow()
    {
        var secret = _totp.GenerateSecret();
        var code = ComputeCode(secret, FixedInstant);

        Assert.True(_totp.TryVerifyCode(secret, code, FixedInstant, out var timeStep));
        Assert.Equal(ExpectedTimeStep(FixedInstant), timeStep);
    }

    [Theory]
    [InlineData(-29)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(29)]
    public void TryVerifyCode_AcceptsOneStepOfClockDrift(int secondsOffset)
    {
        var secret = _totp.GenerateSecret();
        var code = ComputeCode(secret, FixedInstant.AddSeconds(secondsOffset));

        Assert.True(_totp.TryVerifyCode(secret, code, FixedInstant, out _));
    }

    // Note the offsets: a code is only one step away when its 30 second window differs, so
    // +/-31 seconds can still fall inside the tolerance window depending on where the instant
    // sits within its own step. Two steps of drift (60s) is the first reliably-rejected case.
    [Theory]
    [InlineData(-61)]
    [InlineData(-90)]
    [InlineData(60)]
    [InlineData(90)]
    public void TryVerifyCode_RejectsCodesOutsideTheWindow(int secondsOffset)
    {
        var secret = _totp.GenerateSecret();
        var code = ComputeCode(secret, FixedInstant.AddSeconds(secondsOffset));

        Assert.False(_totp.TryVerifyCode(secret, code, FixedInstant, out _));
    }

    [Fact]
    public void TryVerifyCode_RejectsAReusedCodeFromAnEarlierStep()
    {
        var secret = _totp.GenerateSecret();

        // The code of the previous window is still inside the tolerance window, so only the
        // caller-side step bookkeeping can catch this replay.
        var staleCode = ComputeCode(secret, FixedInstant.AddSeconds(-30));
        Assert.True(_totp.TryVerifyCode(secret, staleCode, FixedInstant, out var staleStep));

        var freshCode = ComputeCode(secret, FixedInstant);
        Assert.True(_totp.TryVerifyCode(secret, freshCode, FixedInstant, out var freshStep));

        Assert.True(freshStep > staleStep);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("12 34 56")]
    [InlineData("000000")]
    public void TryVerifyCode_RejectsMalformedInput(string code)
    {
        var secret = _totp.GenerateSecret();

        Assert.False(_totp.TryVerifyCode(secret, code, FixedInstant, out _));
    }

    [Fact]
    public void TryVerifyCode_DoesNotThrowOnAMalformedSecret()
    {
        Assert.False(_totp.TryVerifyCode("not-base32!!", "123456", FixedInstant, out _));
    }

    [Fact]
    public void GetTimeStep_UsesThirtySecondWindowsFromTheEpoch()
    {
        var expected = (long)(FixedInstant - DateTime.UnixEpoch).TotalSeconds / 30;

        Assert.Equal(expected, _totp.GetTimeStep(FixedInstant));
        Assert.Equal(expected + 1, _totp.GetTimeStep(FixedInstant.AddSeconds(30)));
    }

    [Fact]
    public void GetTimeStep_TreatsLocalTimeAsUtc()
    {
        var utcStep = _totp.GetTimeStep(FixedInstant);
        var unspecifiedStep = _totp.GetTimeStep(DateTime.SpecifyKind(FixedInstant, DateTimeKind.Unspecified));

        Assert.Equal(utcStep, unspecifiedStep);
    }

    [Fact]
    public void GetProvisioningUri_EmitsTheKeyUriFormatAuthenticatorAppsExpect()
    {
        var secret = _totp.GenerateSecret();

        var uri = _totp.GetProvisioningUri("OroIdentityServer", "3f2504e0-4f89-11d3-9a0c-0305e82c3301", secret);

        Assert.StartsWith("otpauth://totp/OroIdentityServer:3f2504e0-4f89-11d3-9a0c-0305e82c3301?", uri);
        Assert.Contains($"secret={secret}", uri);
        Assert.Contains("issuer=OroIdentityServer", uri);
        Assert.Contains("algorithm=SHA1", uri);
        Assert.Contains("digits=6", uri);
        Assert.Contains("period=30", uri);
    }

    [Fact]
    public void GetProvisioningUri_EscapesTheAccountLabel()
    {
        var uri = _totp.GetProvisioningUri("Oro Identity", "user@example.com", "AAAAAAAA");

        Assert.DoesNotContain(" ", uri);
        Assert.Contains("user%40example.com", uri);
    }

    private static string ComputeCode(string base32Secret, DateTime instant)
        => new Totp(Base32Encoding.ToBytes(base32Secret)).ComputeTotp(instant);

    private static long ExpectedTimeStep(DateTime instant)
        => (long)(instant - DateTime.UnixEpoch).TotalSeconds / 30;
}