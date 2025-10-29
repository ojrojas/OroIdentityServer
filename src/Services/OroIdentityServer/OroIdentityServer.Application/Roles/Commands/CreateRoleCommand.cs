// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to create a new role in the system.
/// </summary>
public class CreateRoleCommand : ICommand
{
    /// <summary>
    /// Gets or sets the name of the role to be created.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the unique correlation identifier for this command instance.
/// </summary>
    public Guid CorrelationId() => Guid.NewGuid();
}
