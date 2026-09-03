namespace OroIdentityServer.Application.Modules.Users.Commands;

public class DeactivateUserCommandHandler(
    ILogger<DeactivateUserCommandHandler> logger,
    IUserRepository userRepository
) : ICommandHandler<DeactivateUserCommand>
{
    public async Task<Result> HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeactivateUserCommand for Id: {Id}", command.Id);

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
            logger.LogError(ex, "Error deactivating user {Id}", command.Id);
            throw;
        }
    }
}
