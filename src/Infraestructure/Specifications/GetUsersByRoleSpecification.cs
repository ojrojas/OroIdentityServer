// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUsersByRoleSpecification : Specification<User>
{
    public GetUsersByRoleSpecification(RoleId roleId, TenantId? tenantId)
        : base(u => u.Roles.Any(r => r.RoleId == roleId)
            && (tenantId == null || u.TenantId == tenantId))
    {
        AddInclude(x => x.Roles);
        AddInclude("Roles.Role");
    }
}
