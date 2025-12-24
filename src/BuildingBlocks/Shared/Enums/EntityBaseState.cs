// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Enums;

/// <summary>
/// Defines the base states for an entity.
/// </summary>
public enum EntityBaseState
{
    INACTIVE = 0,
    ACTIVE,
    MODIFIED,
    DELETED
}