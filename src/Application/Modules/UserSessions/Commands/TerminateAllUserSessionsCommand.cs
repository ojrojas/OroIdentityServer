namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public sealed record TerminateAllUserSessionsCommand(
    Guid UserId
) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
