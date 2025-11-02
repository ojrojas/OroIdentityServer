// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class DeleteUserHanlderCommand(
    ILogger<DeleteUserHanlderCommand> logger,
    IUserRepository userRepository
) : ICommandHandler<DeleteUserCommand>
{
    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
         if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling DeleteUserCommand for Id: {Id}", command.Id);

        try
        {
            // Delete the User object
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Delete User object for Id: {Id}", command.Id);

            await userRepository.DeleteUserAsync(command.Id,cancellationToken );

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Successfully handled DeleteUserCommand for Id: {Id}", command.Id);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "An error occurred while handling DeleteUserCommand for Id: {Id}", command.Id);
            throw;
        }
    }
}