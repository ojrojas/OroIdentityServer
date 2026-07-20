using OroIdentityServer.Core.Modules.UserSessions.Repositories;

namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public sealed class TerminateAllUserSessionsCommandHandler(
    ILogger<TerminateAllUserSessionsCommandHandler> logger,
    IUserSessionRepository userSessionRepository
) : ICommandHandler<TerminateAllUserSessionsCommand>
{
    public async Task<Result> HandleAsync(TerminateAllUserSessionsCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TerminateAllUserSessionsCommand for UserId: {UserId}", command.UserId);
        try
        {
            var sessions = await userSessionRepository.GetSessionsByUserIdAsync(new(command.UserId), cancellationToken);
            foreach (var session in sessions)
            {
                session.DeactivateSession();
                await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
            }

            logger.LogInformation("Terminated {Count} sessions for UserId: {UserId}", sessions.Count(), command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error terminating all sessions for UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
