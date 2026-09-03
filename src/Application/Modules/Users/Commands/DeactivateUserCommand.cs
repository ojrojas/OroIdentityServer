namespace OroIdentityServer.Application.Modules.Users.Commands;

public record DeactivateUserCommand(Guid Id) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
