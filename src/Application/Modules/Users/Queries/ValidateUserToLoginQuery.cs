namespace OroIdentityServer.Application.Modules.Users.Queries;

public record ValidateUserToLoginQuery(string LoginIdentifier) : IQuery<bool>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
