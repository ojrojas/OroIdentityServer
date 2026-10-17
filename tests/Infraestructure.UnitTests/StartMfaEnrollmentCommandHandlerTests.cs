// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OroIdentityServer.Application.Modules.Mfa.Commands;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Mfa.Repositories;
using OroIdentityServer.Core.Modules.Users.ValueObjects;
using OroIdentityServer.Infraestructure.Services;

/// <summary>
/// Enrolment is the step that turns a secret into a working factor, so starting a new one on an
/// account that already has a working factor must not silently disable it.
/// </summary>
public class StartMfaEnrollmentCommandHandlerTests
{
    private readonly IMfaCredentialRepository _credentials = Substitute.For<IMfaCredentialRepository>();
    private readonly StartMfaEnrollmentCommandHandler _handler;

    public StartMfaEnrollmentCommandHandlerTests()
    {
        var totp = new TotpService();
        _handler = new StartMfaEnrollmentCommandHandler(
            NullLogger<StartMfaEnrollmentCommandHandler>.Instance,
            _credentials,
            totp,
            new PassthroughSecretProtector(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());
    }

    [Fact]
    public async Task Start_WithoutAnExistingCredential_CreatesADisabledOne()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((MfaCredential?)null);

        var payload = await _handler.HandleAsync(new StartMfaEnrollmentCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.StartsWith("otpauth://totp/", payload.ProvisioningUri);
        Assert.Matches("^[A-Z2-7]{32}$", payload.Secret);

        await _credentials.Received(1).AddAsync(
            Arg.Is<MfaCredential>(c => !c.IsEnabled && c.EncryptedSecret == payload.Secret),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Start_WithAPendingCredential_ReplacesTheSecret()
    {
        var userId = UserId.New();
        var existing = new MfaCredential(userId, "old-secret");
        _credentials.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existing);

        var payload = await _handler.HandleAsync(new StartMfaEnrollmentCommand(userId.Value), CancellationToken.None);

        Assert.NotEqual("old-secret", existing.EncryptedSecret);
        Assert.False(existing.IsEnabled);
        await _credentials.DidNotReceive().AddAsync(Arg.Any<MfaCredential>(), Arg.Any<CancellationToken>());
        await _credentials.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Start_WithAWorkingFactor_IsRefused()
    {
        var userId = UserId.New();
        var existing = new MfaCredential(userId, "old-secret");
        existing.Confirm(DateTime.UtcNow, timeStep: 1_000);
        _credentials.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(new StartMfaEnrollmentCommand(userId.Value), CancellationToken.None));

        // The working factor must survive the refused request.
        Assert.True(existing.IsEnabled);
        Assert.Equal("old-secret", existing.EncryptedSecret);
        await _credentials.DidNotReceive().UpdateAsync(Arg.Any<MfaCredential>(), Arg.Any<CancellationToken>());
    }

    private sealed class PassthroughSecretProtector : IMfaSecretProtector
    {
        public string Protect(string plainSecret) => plainSecret;
        public string Unprotect(string protectedSecret) => protectedSecret;
    }
}