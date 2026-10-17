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
            _passwordHasher);
    }

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