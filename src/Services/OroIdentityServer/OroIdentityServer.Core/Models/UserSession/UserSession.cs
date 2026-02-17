// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class UserSession : BaseEntity<UserSession, UserSessionId>, IAggregateRoot
{
    // Constructor vacío requerido por EF Core
    private UserSession()
    {
        // Constructor vacío para EF Core
    }

    public UserSession(UserId userId, string device, string sessionToken, DateTime expiresAt, string? ipAddress = null, string? userAgent = null, string? location = null)
    {
        Id = UserSessionId.New();
        UserId = userId;
        Device = device;
        SessionToken = sessionToken;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Location = location;
        RaiseDomainEvent(new UserSessionCreatedEvent(Id, userId, device, sessionToken, expiresAt, ipAddress, userAgent, location)); 
    }

    public static UserSession CreateNewSession(UserId userId, string device, string sessionToken, DateTime expiresAt, string? ipAddress = null, string? userAgent = null, string? location = null)
    {
        return new UserSession(userId, device, sessionToken, expiresAt, ipAddress, userAgent, location);
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserSessionUpdatedEvent(Id, LastActivityAt, ExpiresAt, IpAddress, UserAgent, Location));
    }

    public void DeactivateSession()
    {
        ExpiresAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserSessionDeactivatedEvent(Id));
    }

    public UserId? UserId { get; private set; }
    public string? Device { get; private set; }
    public string? SessionToken { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Location { get; private set; }
}
