// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetTenantsByUserIdSpecification : Specification<Tenant>
{
    public GetTenantsByUserIdSpecification(UserId userId)
        : base(t => t.TenantUsers.Any(tu => tu.UserId == userId && tu.IsActive))
    {
        AddInclude(t => t.TenantUsers);
    }
}
