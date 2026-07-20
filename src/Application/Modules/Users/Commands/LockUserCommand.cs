namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed record LockUserCommand(
    Guid UserId
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
