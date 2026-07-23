namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed class UnlockUserCommandHandler(
    ILogger<UnlockUserCommandHandler> logger,
    IUserRepository userRepository
) : ICommandHandler<UnlockUserCommand>
{
    public async Task<Result> HandleAsync(UnlockUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling UnlockUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken) ?? throw new InvalidOperationException("User not found.");
            if (user.SecurityUser is null)
                throw new InvalidOperationException("SecurityUser not found for this user.");

            user.SecurityUser.SetLockoutEnabled(false);
            user.SecurityUser.LockUntil(DateTime.UtcNow);
            user.SecurityUser.ResetAccessFailedCount();

            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully unlocked UserId: {UserId}", command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while unlocking UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
