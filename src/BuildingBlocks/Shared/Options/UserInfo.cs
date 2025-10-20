// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Options;

public class UserInfo
{
    public Guid Id { get; set; }
    public required string UserName { get; set; } = string.Empty;
    public EntityBaseState State { get; set; }
    public string Email { get; set; } = string.Empty;
}