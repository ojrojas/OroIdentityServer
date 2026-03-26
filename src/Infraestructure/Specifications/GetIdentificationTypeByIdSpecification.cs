// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;

public sealed class GetIdentificationTypeByIdSpecification(Guid criteria) 
: ISpecification<IdentificationType>
{
    public Expression<Func<IdentificationType, bool>> Criteria { get; } = x => EF.Equals(x.Id.Value, criteria);
    public List<Expression<Func<IdentificationType, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
}