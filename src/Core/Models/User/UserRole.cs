// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public class UserRole : IAggregateRoot
{
    // Constructor vacío requerido por EF Core
    private UserRole()
    {
        // Constructor vacío para EF Core
    }

    public UserRole(UserId userId, RoleId roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public UserId? UserId { get; private set; }
    public RoleId? RoleId { get; private set; }
}