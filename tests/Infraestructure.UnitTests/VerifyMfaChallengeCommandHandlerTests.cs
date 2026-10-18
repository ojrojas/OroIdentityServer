// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using BuildingBlocks.Kernel.Results;
using NSubstitute;
using OroIdentityServer.Application.Modules.Mfa.Commands;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Mfa;
using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.Repositories;
using OroIdentityServer.Core.Modules.UserSessions.Repositories;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// Behaviour of the gate between "password accepted" and "session issued". The collaborators are
/// substituted so the outcome is fully deterministic: a TOTP window is 30 seconds wide and a test
/// suite must not depend on where in that window it happens to run.
/// </summary>
public class VerifyMfaChallengeCommandHandlerTests
{
    private readonly IMfaLoginChallengeRepository _challenges = Substitute.For<IMfaLoginChallengeRepository>();
    private readonly IMfaCredentialRepository _credentials = Substitute.For<IMfaCredentialRepository>();
    private readonly IMfaRecoveryCodeRepository _recoveryCodes = Substitute.For<IMfaRecoveryCodeRepository>();
    private readonly ITotpService _totp = Substitute.For<ITotpService>();
    private readonly IMfaSecretProtector _protector = Substitute.For<IMfaSecretProtector>();
    private readonly IRecoveryCodeGenerator _recoveryGenerator = Substitute.For<IRecoveryCodeGenerator>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly VerifyMfaChallengeCommandHandler _handler;

    public VerifyMfaChallengeCommandHandlerTests()
    {
        _protector.Unprotect(Arg.Any<string>()).Returns("plain-secret");
        _recoveryCodes.GetUnusedByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MfaRecoveryCode>());

        _handler = new VerifyMfaChallengeCommandHandler(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<VerifyMfaChallengeCommandHandler>.Instance,
            _challenges,
            _credentials,
            _recoveryCodes,
            _totp,
            _protector,
            _recoveryGenerator,
            _passwordHasher,
            Microsoft.Extensions.Options.Options.Create(_policy));
    }

    /// <summary>
    /// Deliberately generous so the per-challenge tests keep exercising the challenge budget:
    /// the credential-level lockout has its own tests below with a tight budget.
    /// </summary>
    private readonly MfaLockoutPolicy _policy = new()
    {
        MaxAttemptsPerWindow = 100,
        AttemptWindow = TimeSpan.FromMinutes(15),
        LockoutDuration = TimeSpan.FromMinutes(15)
    };

    /// <summary>
    /// Stubs the TOTP verification, including the out parameter: the handler stores the matched
    /// step to refuse replays, so the step is part of the contract under test.
    /// </summary>
    private void TotpReturns(bool valid, long matchedStep = 6_000)
        => _totp.TryVerifyCode(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), out Arg.Any<long>())
            .Returns(call => { call[3] = matchedStep; return valid; });

    [Fact]
    public async Task ValidTotp_CompletesTheChallengeAndReturnsTheUser()
    {
        var (challenge, credential, userId) = Arrange(MfaChallengePurpose.Verify);
        TotpReturns(valid: true);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(MfaChallengeStatus.Completed, challenge.Status);
        Assert.False(result.UsedRecoveryCode);
        await _challenges.Received(1).UpdateAsync(challenge, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnknownChallengeToken_IsRejected()
    {
        _challenges.GetByTokenAsync("nope", Arg.Any<CancellationToken>()).Returns((MfaLoginChallenge?)null);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand("nope", "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.UnknownChallenge, result.FailureReason);
    }

    [Fact]
    public async Task InvalidCode_CountsAsAnAttempt()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Verify);
        TotpReturns(valid: false);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "000000"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(1, challenge.Attempts);
        Assert.Equal(MfaFailureReason.InvalidCode, result.FailureReason);
        Assert.Equal(MfaLoginChallenge.MaxAttempts - 1, result.AttemptsRemaining);
    }

    [Fact]
    public async Task RepeatedWrongCodes_LockTheChallenge()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Verify);
        TotpReturns(valid: false);

        MfaChallengeVerification result = default!;
        for (var i = 0; i < MfaLoginChallenge.MaxAttempts; i++)
            result = await _handler.HandleAsync(new VerifyMfaChallengeCommand(challenge.Token, "000000"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.IsLocked);
        Assert.Equal(MfaFailureReason.ChallengeLocked, result.FailureReason);
        Assert.Equal(MfaChallengeStatus.Locked, challenge.Status);
    }

    [Fact]
    public async Task AfterLocking_EvenAValidCodeIsRefused()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Verify);
        TotpReturns(valid: false);

        for (var i = 0; i < MfaLoginChallenge.MaxAttempts; i++)
            await _handler.HandleAsync(new VerifyMfaChallengeCommand(challenge.Token, "000000"), CancellationToken.None);

        TotpReturns(valid: true);
        var result = await _handler.HandleAsync(new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.IsLocked);
        Assert.Equal(MfaChallengeStatus.Locked, challenge.Status);
    }

    [Fact]
    public async Task ReplayedTotp_IsRejectedOnceTheStepIsSpent()
    {
        var (challenge, credential, _) = Arrange(MfaChallengePurpose.Verify);
        credential.Confirm(DateTime.UtcNow, timeStep: 5_000);

        // A code that matches an already-consumed step is a replay, not a fresh proof.
        TotpReturns(valid: true, matchedStep: 4_999);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(1, challenge.Attempts);
    }

    [Fact]
    public async Task ExpiredChallenge_IsRejectedWithoutIssuingAnything()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Verify);
        challenge.RegisterFailedAttempt(DateTime.UtcNow.AddMinutes(10)); // forces Locked/expired state

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.IsLocked);
    }

    [Fact]
    public async Task DisabledCredential_CannotBeUsedToSignIn()
    {
        var (challenge, credential, _) = Arrange(MfaChallengePurpose.Verify);
        credential.Disable();
        TotpReturns(valid: true);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.NotEnrolled, result.FailureReason);
    }

    [Fact]
    public async Task EnrolmentChallenge_MustNotBeCompletedByAVerification()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Enroll);
        TotpReturns(valid: true);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.NotEnrolled, result.FailureReason);
        Assert.Equal(0, challenge.Attempts);
    }

    [Fact]
    public async Task RecoveryCode_IsConsumedOnceAndCompletesTheChallenge()
    {
        var (challenge, credential, userId) = Arrange(MfaChallengePurpose.Verify);
        var code = new MfaRecoveryCode(credential.Id, credential.UserId, "hashed");

        _recoveryGenerator.Normalize("ABCD-EF1234").Returns("ABCDEF1234");
        _recoveryCodes.GetUnusedByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(new[] { code });
        _passwordHasher.VerifyPassword("ABCDEF1234", "hashed").Returns(true);
        TotpReturns(valid: false);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "ABCD-EF1234"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.UsedRecoveryCode);
        Assert.Equal(userId, result.UserId);
        Assert.True(code.IsUsed);
        Assert.Equal(MfaChallengeStatus.Completed, challenge.Status);
    }

    [Fact]
    public async Task UsedRecoveryCode_IsNotAcceptedTwice()
    {
        var (challenge, credential, _) = Arrange(MfaChallengePurpose.Verify);
        var used = new MfaRecoveryCode(credential.Id, credential.UserId, "hashed");
        used.MarkUsed(DateTime.UtcNow);

        _recoveryGenerator.Normalize(Arg.Any<string>()).Returns("ABCDEF1234");
        _recoveryCodes.GetUnusedByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MfaRecoveryCode>()); // the repository only returns unused ones
        TotpReturns(valid: false);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "ABCDEF1234"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.InvalidCode, result.FailureReason);
    }

    [Fact]
    public async Task UnknownRecoveryCode_FallsBackToAFailedAttempt()
    {
        var (challenge, credential, _) = Arrange(MfaChallengePurpose.Verify);

        _recoveryGenerator.Normalize(Arg.Any<string>()).Returns("ABCDEF1234");
        var candidate = new MfaRecoveryCode(credential.Id, credential.UserId, "other-hash");
        _recoveryCodes.GetUnusedByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(new[] { candidate });
        _passwordHasher.VerifyPassword("ABCDEF1234", "other-hash").Returns(false);
        TotpReturns(valid: false);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "ABCDEF1234"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(1, challenge.Attempts);
        Assert.False(candidate.IsUsed);
    }

    [Fact]
    public async Task CompletedChallenge_CannotBeReplayed()
    {
        var (challenge, _, _) = Arrange(MfaChallengePurpose.Verify);
        challenge.Complete(DateTime.UtcNow);
        TotpReturns(valid: true);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.UnknownChallenge, result.FailureReason);
    }

    // ---------------------------------------------------------------------------
    // Time-based lockout: the per-challenge budget is defeated by retyping the password, so a
    // second, per-credential budget is what actually bounds the search space.
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task WrongCodesAcrossFreshChallenges_LockTheCredentialOut()
    {
        _policy.MaxAttemptsPerWindow = 3;
        var userId = Guid.NewGuid();
        var credential = ConfirmedCredential(userId);
        TotpReturns(valid: false);

        MfaChallengeVerification result = default!;
        for (var i = 0; i < _policy.MaxAttemptsPerWindow; i++)
        {
            // A brand new challenge each time, exactly what retyping the password produces: the
            // per-challenge counter is back to zero every round, but the credential one is not.
            var challenge = NewChallenge(userId, credential);
            result = await _handler.HandleAsync(
                new VerifyMfaChallengeCommand(challenge.Token, "000000"), CancellationToken.None);
        }

        Assert.False(result.Succeeded);
        Assert.True(result.IsLocked);
        Assert.Equal(MfaFailureReason.LockedOut, result.FailureReason);
        Assert.Equal((int)TimeSpan.FromMinutes(15).TotalSeconds, result.RetryAfterSeconds);
        Assert.True(credential.IsLockedOut(DateTime.UtcNow));
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task WhileLockedOut_NotEvenAValidCodeIsEvaluated()
    {
        _policy.MaxAttemptsPerWindow = 1;
        var userId = Guid.NewGuid();
        var credential = ConfirmedCredential(userId);
        TotpReturns(valid: false);

        var first = NewChallenge(userId, credential);
        await _handler.HandleAsync(new VerifyMfaChallengeCommand(first.Token, "000000"), CancellationToken.None);

        TotpReturns(valid: true);
        var second = NewChallenge(userId, credential);
        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(second.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.LockedOut, result.FailureReason);
        Assert.NotEqual(MfaChallengeStatus.Completed, second.Status);
    }

    [Fact]
    public async Task ASuccessfulCode_ClearsTheFailureCounters()
    {
        _policy.MaxAttemptsPerWindow = 5;
        var userId = Guid.NewGuid();
        var credential = ConfirmedCredential(userId);
        TotpReturns(valid: false);

        var first = NewChallenge(userId, credential);
        await _handler.HandleAsync(new VerifyMfaChallengeCommand(first.Token, "000000"), CancellationToken.None);
        Assert.Equal(1, credential.FailedAttempts);

        TotpReturns(valid: true);
        var second = NewChallenge(userId, credential);
        await _handler.HandleAsync(new VerifyMfaChallengeCommand(second.Token, "123456"), CancellationToken.None);

        Assert.Equal(0, credential.FailedAttempts);
        Assert.Null(credential.LastFailedAtUtc);
        Assert.False(credential.IsLockedOut(DateTime.UtcNow));
    }

    private static MfaCredential ConfirmedCredential(Guid userId)
    {
        var credential = new MfaCredential(new(userId), "protected-secret");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);
        return credential;
    }

    /// <summary>Same user and credential, fresh challenge: one round of password + second factor.</summary>
    private MfaLoginChallenge NewChallenge(Guid userId, MfaCredential credential)
    {
        var challenge = new MfaLoginChallenge(
            new(userId), MfaChallengePurpose.Verify, DateTime.UtcNow.AddMinutes(5), "/users");

        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(credential);
        _challenges.GetByTokenAsync(challenge.Token, Arg.Any<CancellationToken>()).Returns(challenge);

        return challenge;
    }

    [Fact]
    public async Task AnUndecryptableSecret_RequestsReenrolmentInsteadOfThrowing()
    {
        // The Data Protection key ring was rotated after enrolment: the raw CryptographicException
        // used to escape and surface as a 500 on the MFA page.
        _protector.Unprotect(Arg.Any<string>())
            .Returns(_ => throw new OroIdentityServer.Core.Modules.Mfa.Exceptions.MfaSecretUnavailableException("key not found"));

        var (challenge, credential, userId) = Arrange(MfaChallengePurpose.Verify);

        var result = await _handler.HandleAsync(
            new VerifyMfaChallengeCommand(challenge.Token, "123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(MfaFailureReason.SecretUnavailable, result.FailureReason);
        Assert.True(result.RequiresEnrollment);
        Assert.Equal(userId, result.UserId);
        // Demoted, so the next login is routed to enrolment instead of a prompt that can never pass.
        Assert.False(credential.IsEnabled);
        Assert.Equal(0, challenge.Attempts);
        await _credentials.Received(1).UpdateAsync(credential, Arg.Any<CancellationToken>());
    }

    private (MfaLoginChallenge Challenge, MfaCredential Credential, Guid UserId) Arrange(MfaChallengePurpose purpose)
    {
        var userId = Guid.NewGuid();
        var credential = new MfaCredential(new(userId), "protected-secret");
        credential.Confirm(DateTime.UtcNow, timeStep: 1_000);

        var challenge = new MfaLoginChallenge(
            new(userId), purpose, DateTime.UtcNow.AddMinutes(5), "/users");

        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(credential);
        _challenges.GetByTokenAsync(challenge.Token, Arg.Any<CancellationToken>()).Returns(challenge);

        return (challenge, credential, userId);
    }
}