// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Sessions.Commands;

/// <summary>
/// Command to create a new session for a user.
/// </summary>
public record CreateSessionCommand(
    Guid UserId,
    string IpAddress,
    string Country,
    Guid TenantId,
    string? AuthorizationId = null
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
