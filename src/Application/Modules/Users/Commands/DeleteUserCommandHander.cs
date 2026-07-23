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
            // Validate if user exists
            var user = await userRepository.GetUserByIdAsync(new(command.Id), cancellationToken) ?? throw new InvalidOperationException("User not found.");

            // Delete the user
            await userRepository.DeleteUserAsync(new(command.Id), cancellationToken);

            logger.LogInformation("Successfully handled DeleteUserCommand for Id: {Id}", command.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling DeleteUserCommand for Id: {Id}", command.Id);
            throw;
        }
    }
}