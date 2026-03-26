// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Command to create a new session for a user.
/// </summary>
public record CreateSessionCommand(
    UserId UserId,
    string IpAddress,
    string Country,
    TenantId TenantId,
    string? AuthorizationId = null
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
