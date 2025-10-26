// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Handles the creation of a new user by processing the <see cref="CreateUserCommand"/>.
/// </summary>
/// <remarks>
/// This command handler performs the following steps:
/// <list type="bullet">
/// <item><description>Logs the initiation of the command handling process.</description></item>
/// <item><description>Hashes the provided password using the <see cref="IPasswordHasher"/>.</description></item>
/// <item><description>Creates a new <see cref="User"/> object with the provided details and hashed password.</description></item>
/// <item><description>Adds the created user to the repository using the <see cref="IUserRepository"/>.</description></item>
/// <item><description>Logs the successful handling of the command.</description></item>
/// </list>
/// If an error occurs during the process, it is logged, and the exception is rethrown.
/// </remarks>
/// <param name="logger">The logger instance for logging information, debug messages, and errors.</param>
/// <param name="userRepository">The repository interface for user-related data operations.</param>
/// <param name="passwordHasher">The service used to hash user passwords securely.</param>
public class CreateUserCommandHandler(
    ILogger<CreateUserCommandHandler> logger,
    IUserRepository userRepository, 
    IPasswordHasher passwordHasher) 
: ICommandHandler<CreateUserCommand>
{
    public async Task HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateUserCommand for UserName: {UserName}", command.UserName);

        try
        {
            // Hash the password
            logger.LogDebug("Hashing password for UserName: {UserName}", command.UserName);
            var passwordHash = await passwordHasher.HashPassword(command.Password);

            // Create the User object
            logger.LogDebug("Creating User object for UserName: {UserName}", command.UserName);
            var user = new User
            {
                UserName = command.UserName,
                Email = command.Email,
                IdentificationTypeId = command.IdentificationTypeId,
                SecurityUser = new SecurityUser
                {
                    PasswordHash = passwordHash,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid()
                }
            };

            // Add the user to the repository
            logger.LogDebug("Adding User to repository for UserName: {UserName}", command.UserName);
            await userRepository.AddUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully handled CreateUserCommand for UserName: {UserName}", command.UserName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling CreateUserCommand for UserName: {UserName}", command.UserName);
            throw;
        }
    }
}



