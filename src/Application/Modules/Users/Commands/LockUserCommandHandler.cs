namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed class LockUserCommandHandler(
    ILogger<LockUserCommandHandler> logger,
    IUserRepository userRepository,
    ISecurityUserRepository securityUserRepository
) : ICommandHandler<LockUserCommand>
{
    public async Task<Result> HandleAsync(LockUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling LockUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User not found with UserId: {UserId}", command.UserId);
                return Result.Failure(Error.NotFound("UserNotFound", "User not found."));
            }

            if (user.SecurityUserId is null)
                return Result.Failure(Error.NotFound("SecurityUserNotFound", "SecurityUser not found for this user."));

            user.SecurityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId.Value, cancellationToken);

            if (user.SecurityUser is null)
                return Result.Failure(Error.NotFound("SecurityUserNotFound", "SecurityUser not found for this user."));

            user.SecurityUser.SetLockoutEnabled(true);
            user.SecurityUser.LockUntil(DateTime.MaxValue);

            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully locked UserId: {UserId}", command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while locking UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
