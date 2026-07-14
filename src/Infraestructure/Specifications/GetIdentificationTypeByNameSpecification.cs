// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.IdentificationTypes.ValueObjects;

namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetIdentificationTypeByNameSpecification(IdentificationTypeName criteria)
    : ISpecification<IdentificationType>
{
    public Expression<Func<IdentificationType, bool>> Criteria { get; } = x => x.Name != null && x.Name.Value == criteria.Value;

    public bool IsSatisfiedBy(IdentificationType entity)
    {
        return Criteria.Compile()(entity);
    }

    public Expression<Func<IdentificationType, bool>> ToExpression()
    {
        return Criteria;
    }
}
