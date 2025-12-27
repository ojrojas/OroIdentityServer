// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;

public sealed class GetIdentificationTypeByNameSpecification(string criteria) 
: ISpecification<IdentificationType>
{
    public Expression<Func<IdentificationType, bool>> Criteria { get; } = x => EF.Functions.Like(x.Name.Value, $"%{criteria}%");
    public List<Expression<Func<IdentificationType, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
}