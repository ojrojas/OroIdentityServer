// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>
/// Administrative reset: drops another user's factor entirely so they are sent through
/// enrolment again on their next login. No code or password is required from the operator,
/// which is why it is a separate command from the self-service disable.
/// </summary>
public record ResetMfaCommand(Guid UserId) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}