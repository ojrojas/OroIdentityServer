using OpenIddict.Abstractions;

namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public sealed class TerminateAllUserSessionsCommandHandler(
    ILogger<TerminateAllUserSessionsCommandHandler> logger,
    IUserSessionRepository userSessionRepository,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictTokenManager tokenManager
) : ICommandHandler<TerminateAllUserSessionsCommand>
{
    public async Task<Result> HandleAsync(TerminateAllUserSessionsCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TerminateAllUserSessionsCommand for UserId: {UserId}", command.UserId);
        try
        {
            // Revoke all OpenIddict authorizations and tokens for this user
            var subject = command.UserId.ToString();
            await foreach (var authorization in authorizationManager.FindBySubjectAsync(subject, cancellationToken))
            {
                try { await authorizationManager.TryRevokeAsync(authorization, cancellationToken); }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke authorization for subject {Subject}", subject); }
            }

            await foreach (var token in tokenManager.FindBySubjectAsync(subject, cancellationToken))
            {
                try { await tokenManager.TryRevokeAsync(token, cancellationToken); }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke token for subject {Subject}", subject); }
            }

            // Deactivate all UserSessions
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
