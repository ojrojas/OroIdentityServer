// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Sessions.Commands;

/// <summary>
/// Command to terminate an existing session.
/// </summary>
public record TerminateSessionCommand(
    Guid SessionId
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
