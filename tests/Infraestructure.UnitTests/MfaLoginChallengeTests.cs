// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// The attempt budget is the whole reason the challenge lives in the database: a six digit TOTP
/// code has ~10^6 values, so an uncapped retry loop would be a real attack.
/// </summary>
public class MfaLoginChallengeTests
{
    [Fact]
    public void RegisterFailedAttempt_LocksTheChallengeOnTheFifthTry()
    {
        var challenge = NewChallenge();

        for (var attempt = 1; attempt < MfaLoginChallenge.MaxAttempts; attempt++)
            Assert.True(challenge.RegisterFailedAttempt(DateTime.UtcNow));

        Assert.Equal(MfaChallengeStatus.Failed, challenge.Status);
        Assert.Equal(MfaLoginChallenge.MaxAttempts - 1, challenge.Attempts);

        Assert.False(challenge.RegisterFailedAttempt(DateTime.UtcNow));

        Assert.Equal(MfaChallengeStatus.Locked, challenge.Status);
        Assert.Equal(MfaLoginChallenge.MaxAttempts, challenge.Attempts);
        Assert.False(challenge.CanBeUsed(DateTime.UtcNow));
    }

    [Fact]
    public void RegisterFailedAttempt_DoesNotCountPastTheBudget()
    {
        var challenge = NewChallenge();

        for (var i = 0; i < MfaLoginChallenge.MaxAttempts + 3; i++)
            challenge.RegisterFailedAttempt(DateTime.UtcNow);

        Assert.Equal(MfaLoginChallenge.MaxAttempts, challenge.Attempts);
    }

    [Fact]
    public void RegisterFailedAttempt_AfterExpiry_LocksWithoutCounting()
    {
        var challenge = NewChallenge();

        var canRetry = challenge.RegisterFailedAttempt(DateTime.UtcNow.AddMinutes(10));

        Assert.False(canRetry);
        Assert.Equal(MfaChallengeStatus.Locked, challenge.Status);
        Assert.Equal(0, challenge.Attempts);
    }

    [Fact]
    public void CanBeUsed_IsFalseOnceExpired()
    {
        var challenge = NewChallenge();

        Assert.True(challenge.CanBeUsed(DateTime.UtcNow));
        Assert.False(challenge.CanBeUsed(DateTime.UtcNow.AddMinutes(6)));
    }

    [Fact]
    public void Supersede_DoesNotOverwriteACompletedChallenge()
    {
        var challenge = NewChallenge();
        challenge.Complete(DateTime.UtcNow);

        challenge.Supersede();

        Assert.Equal(MfaChallengeStatus.Completed, challenge.Status);
    }

    [Fact]
    public void EachChallengeGetsADistinctToken()
    {
        var first = NewChallenge();
        var second = NewChallenge();

        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(32, first.Token.Length);
    }

    private static MfaLoginChallenge NewChallenge()
        => new(UserId.New(), MfaChallengePurpose.Verify, DateTime.UtcNow.AddMinutes(5));
}