// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
using OroIdentityServer.Core.Modules.UserSessions.Enums;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// The session expiry is what the notification sweep keys off, so the two lifetimes have to agree:
/// the cookie slides, therefore the row must slide too, otherwise a user who never stops working
/// gets signed out (and emailed) at a fixed wall-clock hour.
/// </summary>
public class UserSessionExpiryTests
{
    [Fact]
    public void NewSession_IsOpenAndUnnotified()
    {
        var session = NewSession(DateTime.UtcNow.AddHours(8));

        Assert.Equal(SessionEndReason.None, session.EndReason);
        Assert.Null(session.EndedAtUtc);
        Assert.Null(session.ExpiryNotifiedAtUtc);
        Assert.False(session.IsEnded);
    }

    [Fact]
    public void ExtendExpiry_PushesTheDeadlineForward()
    {
        var session = NewSession(DateTime.UtcNow.AddHours(8));
        var newExpiry = DateTime.UtcNow.AddHours(16);

        session.ExtendExpiry(newExpiry);

        Assert.Equal(newExpiry, session.ExpiresAt);
        Assert.NotNull(session.LastActivityAt);
        Assert.Equal(SessionEndReason.None, session.EndReason);
    }

    [Fact]
    public void ExtendExpiry_DoesNotResurrectAClosedSession()
    {
        var session = NewSession(DateTime.UtcNow.AddHours(8));
        session.DeactivateSession(SessionEndReason.LoggedOut);
        var closedAt = session.ExpiresAt;

        session.ExtendExpiry(DateTime.UtcNow.AddHours(16));

        Assert.Equal(closedAt, session.ExpiresAt);
        Assert.Equal(SessionEndReason.LoggedOut, session.EndReason);
        Assert.True(session.IsEnded);
    }

    [Fact]
    public void MarkExpired_RecordsWhenAndWhyWithoutClobberingARealLogout()
    {
        var session = NewSession(DateTime.UtcNow.AddHours(8));

        session.MarkExpired(DateTime.UtcNow);

        Assert.Equal(SessionEndReason.Expired, session.EndReason);
        Assert.NotNull(session.EndedAtUtc);
        Assert.True(session.IsEnded);

        var loggedOut = NewSession(DateTime.UtcNow.AddHours(8));
        loggedOut.DeactivateSession(SessionEndReason.LoggedOut);
        loggedOut.MarkExpired(DateTime.UtcNow);
        Assert.Equal(SessionEndReason.LoggedOut, loggedOut.EndReason);
    }

    [Fact]
    public void MarkExpiryNotified_MakesTheSweepIdempotent()
    {
        var session = NewSession(DateTime.UtcNow.AddMinutes(-1));
        var notifiedAt = DateTime.UtcNow;

        session.MarkExpired(notifiedAt);
        session.MarkExpiryNotified(notifiedAt);

        Assert.Equal(notifiedAt, session.ExpiryNotifiedAtUtc);
    }

    [Fact]
    public void DeactivateSession_DefaultsToLoggedOut()
    {
        var session = NewSession(DateTime.UtcNow.AddHours(8));

        session.DeactivateSession();

        Assert.Equal(SessionEndReason.LoggedOut, session.EndReason);
        Assert.NotNull(session.EndedAtUtc);
    }

    private static UserSession NewSession(DateTime expiresAt)
        => UserSession.CreateNewSession(
            UserId.New(),
            "Chrome on Linux",
            Guid.NewGuid().ToString("N"),
            expiresAt,
            "10.0.0.1",
            "agent",
            null,
            "es-419");
}