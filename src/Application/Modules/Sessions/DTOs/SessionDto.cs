// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Sessions.DTOs;

public class SessionDto
{
    public Guid UserId { get; private set; }
    public User? User { get; set; }
    public Guid TenantId { get; private set; }
    public string? AuthorizationId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
}
