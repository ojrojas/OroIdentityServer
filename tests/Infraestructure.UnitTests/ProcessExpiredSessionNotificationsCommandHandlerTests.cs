// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
using OroIdentityServer.Core.Modules.UserSessions.Enums;
using OroIdentityServer.Core.Modules.UserSessions.Repositories;
using OroIdentityServer.Core.Modules.UserSessions.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// The "your session expired" sweep. Two properties matter: a session that was signed out or
/// revoked by an administrator must never produce one (DeactivateSession used to write the very
/// same ExpiresAt = UtcNow as an idle timeout), and each session may only produce one.
/// </summary>
public class ProcessExpiredSessionNotificationsCommandHandlerTests
{
    private readonly IUserSessionRepository _sessions = Substitute.For<IUserSessionRepository>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly ISessionExpiryEmailComposer _composer = Substitute.For<ISessionExpiryEmailComposer>();
    private readonly ProcessExpiredSessionNotificationsCommandHandler _handler;

    public ProcessExpiredSessionNotificationsCommandHandlerTests()
    {
        _handler = new ProcessExpiredSessionNotificationsCommandHandler(
            NullLogger<ProcessExpiredSessionNotificationsCommandHandler>.Instance,
            _sessions,
            _email,
            _composer);
    }

    [Fact]
    public async Task ExpiredSession_ClosesItAndQueuesOneEmail()
    {
        var session = ExpiredSession();
        GivenPendingNotifications(session);
        _composer.Compose(Arg.Any<ExpiredSessionInfo>()).Returns(Message(session));

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(new ExpiredSessionNotificationSummary(1, 1, 0), summary);
        Assert.Equal(SessionEndReason.Expired, session.EndReason);
        Assert.NotNull(session.EndedAtUtc);
        Assert.NotNull(session.ExpiryNotifiedAtUtc);
        await _sessions.Received(1).UpdateUserSessionAsync(session, Arg.Any<CancellationToken>());
        await _email.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SessionWithoutAnAddress_IsClosedButNotNotified()
    {
        var session = ExpiredSession();
        GivenPendingNotifications(session);
        _composer.Compose(Arg.Any<ExpiredSessionInfo>()).Returns((EmailMessage?)null);

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(new ExpiredSessionNotificationSummary(1, 0, 1), summary);
        Assert.Equal(SessionEndReason.Expired, session.EndReason);
        Assert.NotNull(session.ExpiryNotifiedAtUtc);
        await _email.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignedOutSession_IsNeverReportedAsExpired()
    {
        var session = ExpiredSession();
        session.DeactivateSession(SessionEndReason.LoggedOut); // ExpiresAt is set to UtcNow, like an expiry
        _sessions.GetExpiredPendingNotificationAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ExpiredSessionInfo>());

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(new ExpiredSessionNotificationSummary(0, 0, 0), summary);
        await _email.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokedByAdminSession_IsNeverReportedAsExpired()
    {
        var session = ExpiredSession();
        session.DeactivateSession(SessionEndReason.RevokedByAdmin);
        _sessions.GetExpiredPendingNotificationAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ExpiredSessionInfo>());

        await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        await _email.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SessionAlreadyClosedSinceTheSweepStarted_IsSkipped()
    {
        var session = ExpiredSession();
        GivenPendingNotifications(session);
        // The user signed out between the query and the update.
        _sessions.GetUserSessionByIdAsync(UserSessionId.From(session.Id.Value), Arg.Any<CancellationToken>()).Returns(session);
        session.DeactivateSession(SessionEndReason.LoggedOut);
        _composer.Compose(Arg.Any<ExpiredSessionInfo>()).Returns(Message(session));

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(1, summary.Skipped);
        await _email.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransportFailure_StillStampsTheSessionSoTheSweepMovesOn()
    {
        var session = ExpiredSession();
        GivenPendingNotifications(session);
        _composer.Compose(Arg.Any<ExpiredSessionInfo>()).Returns(Message(session));
        _email.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new IOException("smtp is down"));

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(new ExpiredSessionNotificationSummary(1, 1, 0), summary);
        Assert.NotNull(session.ExpiryNotifiedAtUtc);
    }

    [Fact]
    public async Task NothingToDo_IsANoOp()
    {
        _sessions.GetExpiredPendingNotificationAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ExpiredSessionInfo>());

        var summary = await _handler.HandleAsync(new ProcessExpiredSessionNotificationsCommand(), CancellationToken.None);

        Assert.Equal(new ExpiredSessionNotificationSummary(0, 0, 0), summary);
        await _sessions.DidNotReceive().UpdateUserSessionAsync(Arg.Any<UserSession>(), Arg.Any<CancellationToken>());
    }

    private static UserSession ExpiredSession()
        => UserSession.CreateNewSession(
            UserId.New(),
            "Chrome on Windows",
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.AddMinutes(-1),
            "10.0.0.1",
            "test-agent",
            "Santiago",
            "es-419");

    private void GivenPendingNotifications(UserSession session)
    {
        _sessions.GetExpiredPendingNotificationAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new ExpiredSessionInfo(
                session.Id.Value,
                session.UserId!.Value,
                "jdoe",
                "jdoe@example.com",
                session.Device,
                session.Location,
                session.Culture,
                session.ExpiresAt) });

        _sessions.GetUserSessionByIdAsync(UserSessionId.From(session.Id.Value), Arg.Any<CancellationToken>()).Returns(session);
    }

    private static EmailMessage Message(UserSession session)
        => new("jdoe@example.com", "Your session expired", "<p>body</p>", EmailBodyFormat.Html, "SessionExpired");
}