// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Commands;

/// <summary>
/// Command to record an auth validation audit event (authorize/token/logout outcome).
/// </summary>
public record LogAuthValidationCommand(
    AuthValidationEventType EventType,
    bool Succeeded,
    Guid? UserId,
    string? ClientId,
    string? Scopes,
    string? IpAddress,
    string? FailureReason
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
