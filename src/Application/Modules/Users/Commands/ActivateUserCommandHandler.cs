namespace OroIdentityServer.Application.Modules.Users.Commands;

public class ActivateUserCommandHandler(
    ILogger<ActivateUserCommandHandler> logger,
    IUserRepository userRepository
) : ICommandHandler<ActivateUserCommand>
{
    public async Task<Result> HandleAsync(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ActivateUserCommand for Id: {Id}", command.Id);

        try
        {
            var user = await userRepository.GetUserByIdAsyncIgnoreFilters(new(command.Id), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User not found with Id: {Id}", command.Id);
                return Result.Failure(Error.NotFound("UserNotFound", "User not found."));
            }

            user.Activate();
            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully activated user {Id}", command.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating user {Id}", command.Id);
            throw;
        }
    }
}
