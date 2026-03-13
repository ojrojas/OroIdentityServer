// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateUserSessionCommandHandler(
    ILogger<UpdateUserSessionCommandHandler> logger,
    IUserSessionRepository userSessionRepository
) : ICommandHandler<UpdateUserSessionCommand>
{
    public async Task HandleAsync(UpdateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateUserSessionCommand for SessionId: {SessionId}", command.SessionId);
        try
        {
            var session = await userSessionRepository.GetUserSessionByIdAsync(command.SessionId, cancellationToken);
            if (session == null)
            {
                logger.LogWarning("Session not found: {SessionId}", command.SessionId);
                return;
            }

            session.UpdateLastActivity();
            await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
            logger.LogInformation("Updated LastActivity for SessionId: {SessionId}", command.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating session {SessionId}", command.SessionId);
            throw;
        }
    }
}
