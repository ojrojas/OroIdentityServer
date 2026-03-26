// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class Session : BaseEntity<Session, SessionId>, IAuditableEntity, IAggregateRoot
{
    public UserId UserId { get; private set; }
    public User? User { get; set; }
    public TenantId TenantId { get; private set; }
    public string? AuthorizationId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }

    private Session()
    {
        UserId = default!;
        TenantId = default!;
        AuthorizationId = null;
    }

    public Session(SessionId? id, UserId userId, TenantId tenantId, string ipAddress, string country, DateTime startedAtUtc, string? authorizationId)
    {
        Id = id ?? SessionId.Create(null);
        UserId = userId;
        TenantId = tenantId;
        AuthorizationId = authorizationId;
        IpAddress = ipAddress;
        Country = country;
        StartedAtUtc = startedAtUtc;
    }

    public static Session Create(UserId userId, string ipAddress, string country, TenantId tenantId, string? authorizationId = null)
    {
        return new Session(SessionId.Create(null), userId, tenantId, ipAddress, country, DateTime.UtcNow, authorizationId);
    }

    public void End(DateTime endedAtUtc)
    {
        EndedAtUtc = endedAtUtc;
    }
}
