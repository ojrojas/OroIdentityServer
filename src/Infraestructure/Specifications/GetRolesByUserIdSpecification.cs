// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

/// <summary>
/// Loads the active roles assigned to a user together with their role-permission links.
/// The <see cref="Role"/> query filter already restricts the result to active roles.
/// </summary>
public sealed class GetRolesByUserIdSpecification : Specification<Role>
{
    public GetRolesByUserIdSpecification(IReadOnlyCollection<RoleId> roleIds)
        : base(BuildCriteria(roleIds))
    {
        AddInclude(r => r.RolePermissions);
    }

    private static Expression<Func<Role, bool>> BuildCriteria(IReadOnlyCollection<RoleId> roleIds)
    {
        var ids = roleIds.ToList();
        return r => ids.Contains(r.Id);
    }
}
