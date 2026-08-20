// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUsersByRoleSpecification : Specification<User>
{
    public GetUsersByRoleSpecification(Guid roleId, Guid? tenantId)
        : base(u => u.Roles.Any(r => r.RoleId.Value == roleId)
            && (tenantId == null || u.TenantId != null && u.TenantId.Value == tenantId.Value))
    {
        AddInclude(x => x.Roles);
        AddInclude("Roles.Role");
    }
}
