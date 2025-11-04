// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Logging;

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

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
            var user = await userRepository.GetUserByEmailAsync(command.Username);

            if (user == null || !await passwordHasher.VerifyPassword(command.Password, user.SecurityUser.PasswordHash))
            {
                logger.LogWarning("Invalid login attempt for user {Username}", command.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            logger.LogInformation("User {Username} logged in successfully.", command.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing login for user {Username}.", command.Username);
            throw;
        }
    }
}