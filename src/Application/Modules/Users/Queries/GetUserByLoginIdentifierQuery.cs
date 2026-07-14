namespace OroIdentityServer.Application.Modules.Users.Queries;

public record GetUserByLoginIdentifierQuery(string LoginIdentifier) : IQuery<GetUserByLoginIdentifierResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
