// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class LoginUserCommandHandler(
    ILogger<LoginUserCommandHandler> logger,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : ICommandHandler<LoginUserCommand>
{
    public async Task HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetUserByEmailAsync(command.Username, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Invalid login attempt for user {Username}", command.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (user?.SecurityUser == null)
            {
                logger.LogWarning("User or SecurityUser is null for {Username}", command.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            // Check if the account is locked out
            if (user.SecurityUser.LockoutEnabled && user.SecurityUser.LockoutEnd.HasValue && user.SecurityUser.LockoutEnd.Value > DateTime.UtcNow)
            {
                logger.LogWarning("User {Username} is locked out until {LockoutEnd}", command.Username, user.SecurityUser.LockoutEnd);
                throw new UnauthorizedAccessException("Account is locked. Please try again later.");
            }

            // Verify the password
            if (!await passwordHasher.VerifyPassword(command.Password, user.SecurityUser.PasswordHash!))
            {
                // Increment failed access count
                user.SecurityUser.IncrementAccessFailedCount();

                if (user.SecurityUser.AccessFailedCount >= 5) // Threshold for failed attempts
                {
                    user.SecurityUser.LockUntil(DateTime.UtcNow.AddMinutes(15)); // Lock the account for 15 minutes
                    logger.LogWarning("User {Username} account locked until {LockoutEnd}", command.Username, user.SecurityUser.LockoutEnd);
                }

                await userRepository.UpdateUserAsync(user, cancellationToken);
                logger.LogWarning("Invalid login attempt for user {Username}", command.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            // Reset failed access count on successful login
            user.SecurityUser.ResetAccessFailedCount();
            user.SecurityUser.LockUntil(DateTime.UtcNow); // Clear any lockout
            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("User {Username} logged in successfully.", command.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing login for user {Username}.", command.Username);
            throw;
        }
    }
}