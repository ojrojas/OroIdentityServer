// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserRolesByUserIdSpecification(UserId UserId) : ISpecification<UserRole>
{
    public Expression<Func<UserRole, bool>> Criteria { get; } = x => x.UserId == UserId;

    public bool IsSatisfiedBy(UserRole entity)
    {
        return Criteria.Compile()(entity);
    }

    public Expression<Func<UserRole, bool>> ToExpression()
    {
        return Criteria;
    }
}