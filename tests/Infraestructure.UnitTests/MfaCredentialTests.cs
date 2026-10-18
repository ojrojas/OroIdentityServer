// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Mfa;
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

    [Fact]
    public void RequireReenrollment_DisablesTheFactorAndDropsTheLockoutState()
    {
        var credential = new MfaCredential(UserId.New(), "protected");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);
        credential.RegisterFailedAttempt(DateTime.UtcNow, new MfaLockoutPolicy { MaxAttemptsPerWindow = 1 });

        credential.RequireReenrollment();

        Assert.False(credential.IsEnabled);
        Assert.Equal(0, credential.FailedAttempts);
        Assert.False(credential.IsLockedOut(DateTime.UtcNow));
    }
}

/// <summary>
/// The per-credential brute-force budget. It exists because the per-challenge one is defeated by
/// retyping the password: each new login issues a challenge with a fresh counter, so without this
/// a 6 digit code is only ever rate-limited per page load.
/// </summary>
public class MfaCredentialLockoutTests
{
    private static readonly MfaLockoutPolicy Policy = new()
    {
        MaxAttemptsPerWindow = 3,
        AttemptWindow = TimeSpan.FromMinutes(15),
        LockoutDuration = TimeSpan.FromMinutes(15)
    };

    [Fact]
    public void TheWindowBudgetLocksTheCredentialOnTheLastAttempt()
    {
        var credential = EnabledCredential();

        Assert.False(credential.RegisterFailedAttempt(T0, Policy));
        Assert.False(credential.RegisterFailedAttempt(T0, Policy));
        Assert.True(credential.RegisterFailedAttempt(T0, Policy));

        Assert.True(credential.IsLockedOut(T0));
        Assert.Equal(T0.Add(Policy.LockoutDuration), credential.LockedUntilUtc);
        // The counter resets, otherwise the next lockout would fire on the very next failure.
        Assert.Equal(0, credential.FailedAttempts);
    }

    [Fact]
    public void TheLockoutExpires()
    {
        var credential = EnabledCredential();
        for (var i = 0; i < Policy.MaxAttemptsPerWindow; i++)
            credential.RegisterFailedAttempt(T0, Policy);

        Assert.True(credential.IsLockedOut(T0.AddMinutes(14)));
        Assert.False(credential.IsLockedOut(T0.AddMinutes(16)));
        Assert.Equal(TimeSpan.Zero, Policy.RemainingLockout(T0.AddMinutes(16), credential.LockedUntilUtc));
    }

    [Fact]
    public void FailuresOlderThanTheWindowStartAFreshCount()
    {
        var credential = EnabledCredential();
        credential.RegisterFailedAttempt(T0, Policy);
        credential.RegisterFailedAttempt(T0, Policy);

        // Still inside the window: the third one locks.
        Assert.True(credential.RegisterFailedAttempt(T0.AddMinutes(5), Policy));
        Assert.True(credential.IsLockedOut(T0.AddMinutes(5)));
    }

    [Fact]
    public void AWindowThatHasAgedOutDoesNotCountTowardsTheNewOne()
    {
        var credential = EnabledCredential();
        credential.RegisterFailedAttempt(T0, Policy);
        credential.RegisterFailedAttempt(T0, Policy);

        // 16 minutes later the previous two are outside the sliding window, so the user is back to
        // the full budget - it is a sliding window, not a permanent counter. The two stale
        // failures must not be carried over into the new window.
        var later = T0.AddMinutes(16);
        Assert.Equal(Policy.MaxAttemptsPerWindow, credential.AttemptsRemainingInWindow(later, Policy));

        Assert.False(credential.RegisterFailedAttempt(later, Policy));
        Assert.False(credential.RegisterFailedAttempt(later, Policy));
        Assert.True(credential.RegisterFailedAttempt(later, Policy));
    }

    [Fact]
    public void ASuccessClearsTheCounters()
    {
        var credential = EnabledCredential();
        credential.RegisterFailedAttempt(T0, Policy);
        credential.RegisterFailedAttempt(T0, Policy);

        credential.RecordVerifiedUse(9_999, T0.AddMinutes(1));

        Assert.Equal(0, credential.FailedAttempts);
        Assert.Null(credential.LastFailedAtUtc);
        Assert.Null(credential.LockedUntilUtc);
        Assert.Equal(Policy.MaxAttemptsPerWindow, credential.AttemptsRemainingInWindow(T0, Policy));
    }

    private static readonly DateTime T0 = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static MfaCredential EnabledCredential()
    {
        var credential = new MfaCredential(UserId.New(), "protected");
        credential.Confirm(T0, timeStep: 1_000);
        return credential;
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