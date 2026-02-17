// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class DeactivateUserSessionCommandHandler(
    ILogger<DeactivateUserSessionCommandHandler> logger,
    IUserSessionRepository userSessionRepository
) : ICommandHandler<DeactivateUserSessionCommand>
{
    public async Task HandleAsync(DeactivateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeactivateUserSessionCommand for SessionId: {SessionId}", command.SessionId);
        try
        {
            var session = await userSessionRepository.GetUserSessionByIdAsync(command.SessionId, cancellationToken);
            if (session == null)
            {
                logger.LogWarning("Session not found: {SessionId}", command.SessionId);
                return;
            }

            session.DeactivateSession();
            await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
            logger.LogInformation("Deactivated session {SessionId}", command.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating session {SessionId}", command.SessionId);
            throw;
        }
    }
}
