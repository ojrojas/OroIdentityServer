// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;

internal class GetUserRolesByUserIdSpecification(UserId UserId) : ISpecification<UserRole>
{
    public Expression<Func<UserRole, bool>> Criteria { get; } = x => x.UserId == UserId;
    public List<Expression<Func<UserRole, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
}