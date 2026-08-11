// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetRoleWithPermissionsSpecification : Specification<Role>
{
    public GetRoleWithPermissionsSpecification(RoleId roleId) : base(r => r.Id == roleId)
    {
        AddInclude(r => r.RolePermissions);
    }
}
