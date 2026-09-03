namespace OroIdentityServer.Application.Modules.Users.Commands;

public record ActivateUserCommand(Guid Id) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
