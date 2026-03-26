// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public record LoginUserCommand(string Username, string Password) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}