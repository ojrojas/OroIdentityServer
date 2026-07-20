namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed record AssignRolesToUserCommand(
    Guid UserId,
    List<Guid> RoleIds
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
