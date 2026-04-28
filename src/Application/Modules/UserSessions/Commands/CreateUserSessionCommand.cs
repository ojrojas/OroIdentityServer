// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public record CreateUserSessionCommand(
    Guid UserId,
    string Device,
    string SessionToken,
    DateTime ExpiresAt,
    string? IpAddress,
    string? UserAgent,
    string? Location
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
