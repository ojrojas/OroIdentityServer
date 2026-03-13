// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class CreateUserSessionCommandHandler(
    ILogger<CreateUserSessionCommandHandler> logger,
    IUserSessionRepository userSessionRepository
) : ICommandHandler<CreateUserSessionCommand>
{
    public async Task HandleAsync(CreateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateUserSessionCommand for UserId: {UserId}", command.UserId);
        try
        {
            var session = UserSession.CreateNewSession(command.UserId, command.Device, command.SessionToken, command.ExpiresAt, command.IpAddress, command.UserAgent, command.Location);
            await userSessionRepository.AddUserSessionAsync(session, cancellationToken);
            logger.LogInformation("Created user session {SessionId} for user {UserId}", session.Id, command.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user session for user {UserId}", command.UserId);
            throw;
        }
    }
}
