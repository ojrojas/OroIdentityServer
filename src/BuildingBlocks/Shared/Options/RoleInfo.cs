// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Options;

/// <summary>
/// Represents information about a role.
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}