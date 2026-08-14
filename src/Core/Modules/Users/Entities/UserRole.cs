// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.Roles.Aggregates;

namespace OroIdentityServer.Core.Modules.Users.Entities;

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

    /// <summary>
    /// Navigation to the catalogue <see cref="Role"/>. Populated by EF when callers include
    /// <c>Roles.Role</c> in their query; left null otherwise. Sign-in and the master-admin
    /// detector need the name to map catalogue roles to claims, so the spec used at login
    /// time (and any UI-facing user load) must include this.
    /// </summary>
    public Role? Role { get; set; }
}