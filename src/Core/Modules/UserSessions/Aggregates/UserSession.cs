// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserSessions.Aggregates;

using OroIdentityServer.Core.Modules.UserSessions.Enums;

public class UserSession : AggregateRoot<UserSessionId>
{
    // Constructor vacío requerido por EF Core
    private UserSession()
    {
        // Constructor vacío para EF Core
    }

    public UserSession(
        UserId userId,
        string device,
        string sessionToken,
        DateTime expiresAt,
        string? ipAddress = null,
        string? userAgent = null,
        string? location = null,
        string? culture = null)
    {
        Id = UserSessionId.New();
        UserId = userId;
        Device = device;
        SessionToken = sessionToken;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Location = location;
        Culture = culture;
        EndReason = SessionEndReason.None;
        RaiseDomainEvent(new UserSessionCreatedEvent(Id, userId, device, sessionToken, expiresAt, ipAddress, userAgent, location));
    }

    public static UserSession CreateNewSession(
        UserId userId,
        string device,
        string sessionToken,
        DateTime expiresAt,
        string? ipAddress = null,
        string? userAgent = null,
        string? location = null,
        string? culture = null)
    {
        return new UserSession(userId, device, sessionToken, expiresAt, ipAddress, userAgent, location, culture);
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserSessionUpdatedEvent(Id, LastActivityAt, ExpiresAt, IpAddress, UserAgent, Location));
    }

    /// <summary>
    /// Slides the database expiry forward the same way the authentication cookie slides, so the
    /// session tracks inactivity instead of dying at a fixed wall-clock hour while the user's
    /// browser cookie is still valid.
    /// </summary>
    public void ExtendExpiry(DateTime newExpiresAt)
    {
        if (EndReason != SessionEndReason.None) return;
        ExpiresAt = newExpiresAt;
        LastActivityAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserSessionUpdatedEvent(Id, LastActivityAt, ExpiresAt, IpAddress, UserAgent, Location));
    }

    public void DeactivateSession(SessionEndReason reason = SessionEndReason.LoggedOut)
    {
        ExpiresAt = DateTime.UtcNow;
        EndReason = reason;
        EndedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new UserSessionDeactivatedEvent(Id));
    }

    /// <summary>
    /// Closes a session that ran out of time. Called by the expiry sweep right before it queues
    /// the "your session expired" email, which is what makes the notification one-shot.
    /// </summary>
    public void MarkExpired(DateTime nowUtc)
    {
        if (EndReason != SessionEndReason.None) return;
        EndReason = SessionEndReason.Expired;
        EndedAtUtc = nowUtc;
        RaiseDomainEvent(new UserSessionDeactivatedEvent(Id));
    }

    /// <summary>Stamps the "expiry email already sent" flag so the sweep stays idempotent.</summary>
    public void MarkExpiryNotified(DateTime nowUtc) => ExpiryNotifiedAtUtc = nowUtc;

    public UserId? UserId { get; private set; }
    public string? Device { get; private set; }
    public string? SessionToken { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Location { get; private set; }

    /// <summary>Culture the session was started with, so later emails can be sent in it.</summary>
    public string? Culture { get; private set; }

    public SessionEndReason EndReason { get; private set; } = SessionEndReason.None;
    public DateTime? EndedAtUtc { get; private set; }

    /// <summary>When the "session expired" email was queued; null while it is still pending.</summary>
    public DateTime? ExpiryNotifiedAtUtc { get; private set; }

    /// <summary>A session is over once it expired or was explicitly closed.</summary>
    public bool IsEnded => EndReason != SessionEndReason.None || ExpiresAt <= DateTime.UtcNow;
}