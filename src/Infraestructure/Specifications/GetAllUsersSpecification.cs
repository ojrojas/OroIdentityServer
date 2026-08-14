// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetAllUsersSpecification : Specification<User>
{
    public GetAllUsersSpecification()
    {
        AddInclude(x => x.Roles);
        // Catalogue role names are surfaced in the admin UI (UserDetail.razor assigned roles
        // list) and consumed by the master-admin detector, so the eager load is required.
        AddInclude("Roles.Role");
    }
}
