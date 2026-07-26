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
        AddInclude(x => x.SecurityUser!);
    }
}
