// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByEmailSpecification(string criteria) : ISpecification<User>
{
    public Expression<Func<User, bool>> Criteria { get; } = x => EF.Functions.Like(x.Email, $"%{criteria}%");
}