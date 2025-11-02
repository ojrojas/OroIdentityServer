// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to create a new user.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <returns>A new instance of the <see cref="DeleteUserCommand"/> class.</returns>
public record DeleteUserCommand(
  Guid Id
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}
