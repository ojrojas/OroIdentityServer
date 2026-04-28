// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Sessions.DTOs;

public class SessionDto
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid TenantId { get; set; }
    public string? AuthorizationId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
}
