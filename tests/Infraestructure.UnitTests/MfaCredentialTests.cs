// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Users.ValueObjects;
using OroIdentityServer.Core.Services;
using OroIdentityServer.Infraestructure.Services;

/// <summary>
/// Replay protection: a TOTP window is valid for up to three steps (now +/- 1), so without
/// remembering the last spent step a code could be replayed for up to 90 seconds.
/// </summary>
public class MfaCredentialTests
{
    [Fact]
    public void NewCredential_StartsDisabled()
    {
        var credential = new MfaCredential(UserId.New(), "protected");

        Assert.False(credential.IsEnabled);
        Assert.Null(credential.ConfirmedAtUtc);
        Assert.Equal(-1, credential.LastUsedTimeStep);
    }

    [Fact]
    public void Confirm_EnablesTheFactorAndBurnsTheStep()
    {
        var credential = new MfaCredential(UserId.New(), "protected");

        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);

        Assert.True(credential.IsEnabled);
        Assert.NotNull(credential.ConfirmedAtUtc);
        Assert.Equal(1_000, credential.LastUsedTimeStep);
    }

    [Fact]
    public void RestartEnrollment_ClearsTheConfirmedState()
    {
        var credential = new MfaCredential(UserId.New(), "protected");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);

        credential.RestartEnrollment("other-protected-secret");

        Assert.False(credential.IsEnabled);
        Assert.Null(credential.ConfirmedAtUtc);
        Assert.Equal(-1, credential.LastUsedTimeStep);
    }

    [Fact]
    public void RecordVerifiedUse_NeverMovesTheStepBackwards()
    {
        var credential = new MfaCredential(UserId.New(), "protected");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);

        credential.RecordVerifiedUse(999, DateTime.UtcNow);

        Assert.Equal(1_000, credential.LastUsedTimeStep);
    }

    [Fact]
    public void Disable_StopsTheFactorFromApplying()
    {
        var credential = new MfaCredential(UserId.New(), "protected");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);

        credential.Disable();

        Assert.False(credential.IsEnabled);
        Assert.Null(credential.ConfirmedAtUtc);
    }
}

public class RecoveryCodeGeneratorTests
{
    [Fact]
    public void Generate_ProducesDistinctCodes()
    {
        IRecoveryCodeGenerator generator = new RecoveryCodeGenerator();

        var codes = generator.Generate(10);

        Assert.Equal(10, codes.Count);
        Assert.Equal(10, codes.Select(c => c.PlainCode).Distinct().Count());
        Assert.All(codes, c => Assert.Equal(10, c.PlainCode.Length));
    }

    [Fact]
    public void Generate_DoesNotStoreThePlaintext()
    {
        IRecoveryCodeGenerator generator = new RecoveryCodeGenerator();

        var code = generator.Generate(1)[0];

        Assert.NotEqual(code.PlainCode, code.CodeHash);
        Assert.DoesNotContain(code.PlainCode, code.CodeHash);
    }

    [Theory]
    [InlineData("abcd-ef12 34", "ABCDEF1234")]
    [InlineData("  abcdef1234 ", "ABCDEF1234")]
    public void Normalize_StripsSeparatorsAndUppercases(string input, string expected)
    {
        IRecoveryCodeGenerator generator = new RecoveryCodeGenerator();

        Assert.Equal(expected, generator.Normalize(input));
    }

    [Fact]
    public async Task GeneratedCodesAreVerifiableWithThePasswordHasher()
    {
        var generator = new RecoveryCodeGenerator();
        var hasher = new PasswordHasher();

        var code = generator.Generate(1)[0];

        Assert.True(await hasher.VerifyPassword(generator.Normalize(code.PlainCode), code.CodeHash));
        Assert.False(await hasher.VerifyPassword("WRONGCODE1", code.CodeHash));
    }
}