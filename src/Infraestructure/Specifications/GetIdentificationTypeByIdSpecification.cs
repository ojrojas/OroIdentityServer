// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetIdentificationTypeByIdSpecification(IdentificationTypeId criteria)
: ISpecification<IdentificationType>
{
    public Expression<Func<IdentificationType, bool>> Criteria { get; } = x => x.Id == criteria;

    public bool IsSatisfiedBy(IdentificationType entity)
    {
        return Criteria.Compile()(entity);
    }

    public Expression<Func<IdentificationType, bool>> ToExpression()
    {
        return Criteria;
    }
}