namespace OroIdentityServer.Application.Modules.Roles.Commands;

public record ActivateRoleCommand(Guid Id) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
