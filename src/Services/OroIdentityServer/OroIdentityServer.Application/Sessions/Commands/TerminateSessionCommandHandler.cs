// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class TerminateSessionCommandHandler(
    ILogger<TerminateSessionCommandHandler> logger,
    ISessionRepository sessionRepository)
: ICommandHandler<TerminateSessionCommand>
{
    public async Task HandleAsync(TerminateSessionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling TerminateSessionCommand for SessionId: {SessionId}", command.SessionId);

        try
        {
            var session = await sessionRepository.GetSessionByIdAsync(command.SessionId, cancellationToken);
            if (session == null)
                throw new InvalidOperationException("Session not found.");

            session.End(DateTime.UtcNow);
            await sessionRepository.EndSessionAsync(command.SessionId, DateTime.UtcNow, cancellationToken);

            logger.LogInformation("Successfully terminated session with Id: {SessionId}", command.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while terminating session with Id: {SessionId}", command.SessionId);
            throw;
        }
    }
}
