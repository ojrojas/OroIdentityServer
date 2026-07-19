namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByUserNameOrEmailSpecification(string loginIdentifier) : ISpecification<User>
{
    private readonly string _normalizedLoginIdentifier = loginIdentifier.ToUpperInvariant();

    public Expression<Func<User, bool>> Criteria => x =>
        x.NormalizedUserName == _normalizedLoginIdentifier ||
        x.NormalizedEmail == _normalizedLoginIdentifier;

    public bool IsSatisfiedBy(User entity)
    {
        return Criteria.Compile()(entity);
    }

    public Expression<Func<User, bool>> ToExpression()
    {
        return Criteria;
    }
}
