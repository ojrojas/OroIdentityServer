// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetRolesCreatedTodaySpecification : Specification<Role>
{
    public GetRolesCreatedTodaySpecification(DateTime today)
        : base(r => r.CreatedAtUtc >= today && r.CreatedAtUtc < today.AddDays(1))
    {
    }
}
