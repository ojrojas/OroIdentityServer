// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Options;

public class RoleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}