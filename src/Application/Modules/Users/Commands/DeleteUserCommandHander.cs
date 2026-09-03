// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Commands;

public class DeleteUserCommandHander(
    ILogger<DeleteUserCommandHander> logger,
    IUserRepository userRepository
) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling DeleteUserCommand for Id: {Id}", command.Id);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.Id), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User not found with Id: {Id}", command.Id);
                return Result.Failure(Error.NotFound("UserNotFound", "User not found."));
            }

            user.Deactivate();
            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully deactivated user {Id}", command.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling DeleteUserCommand for Id: {Id}", command.Id);
            throw;
        }
    }
}