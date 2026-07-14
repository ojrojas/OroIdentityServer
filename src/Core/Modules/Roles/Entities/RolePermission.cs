// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Roles.Entities;

public class RolePermission 
{
    private RolePermission()
    {
        RoleId = default!;
        PermissionId = default!;
    }

    public RolePermission(RoleId roleId, PermissionId permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public RoleId RoleId { get; private set; }
    public PermissionId PermissionId { get; private set; }
}
