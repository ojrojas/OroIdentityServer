// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByIdSpecification : Specification<User>
{
    public GetUserByIdSpecification(UserId id) : base(x => x.Id == id)
    {
        AddInclude(x => x.Roles);
        // Catalogue role is required to translate a user's roles into claim names
        // (Admin/Administrator/Manager/Member) at sign-in and in the master-admin check.
        AddInclude("Roles.Role");
        AddInclude(x => x.SecurityUser!);
    }
}
