namespace OroIdentityServer.Application.Modules.Users.Queries;

public record ValidateUserPasswordQuery(string LoginIdentifier, string Password) : IQuery<GetUserPasswordValidResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
