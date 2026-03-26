// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByIdSpecification(UserId id) : ISpecification<User>
{
    public Expression<Func<User, bool>> Criteria { get; } = x => x.Id == id;
    public List<Expression<Func<User, object>>> Includes { get; } = [u => u.Roles];
    public List<string> IncludeStrings { get; } = [];
}
