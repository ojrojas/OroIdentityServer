// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class CreateSessionCommandHandler(
    ILogger<CreateSessionCommandHandler> logger,
    ISessionRepository sessionRepository)
: ICommandHandler<CreateSessionCommand>
{
    public async Task HandleAsync(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling CreateSessionCommand for UserId: {UserId}", command.UserId);

        try
        {
            var session = Session.Create(command.UserId, command.IpAddress, command.Country, command.TenantId, command.AuthorizationId);
            await sessionRepository.AddSessionAsync(session, cancellationToken);
            logger.LogInformation("Successfully created session with Id: {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling CreateSessionCommand for UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
