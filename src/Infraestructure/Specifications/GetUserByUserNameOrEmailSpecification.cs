namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByUserNameOrEmailSpecification(string loginIdentifier) : ISpecification<User>
{
    public Expression<Func<User, bool>> Criteria => x =>
        x.NormalizedUserName == loginIdentifier.ToUpperInvariant() ||
        x.NormalizedEmail == loginIdentifier.ToUpperInvariant();

    public bool IsSatisfiedBy(User entity)
    {
        return Criteria.Compile()(entity);
    }

    public Expression<Func<User, bool>> ToExpression()
    {
        return Criteria;
    }
}
