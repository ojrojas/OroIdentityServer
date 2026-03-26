// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class CreateUserCommandHandler(
    ILogger<CreateUserCommandHandler> logger,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
: ICommandHandler<CreateUserCommand>
{
    public async Task HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling CreateUserCommand for UserName: {UserName}", command.UserName);

        try
        {
            // Validate if user already exists
            var existingUser = await userRepository.GetUserByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null)
                throw new InvalidOperationException("User with the given email already exists.");

            // Create the User object
            var user = User.Create(
                command.UserName,
                command.Email,
                command.Name,
                command.MiddleName,
                command.LastName,
                command.Identification,
                command.IdentificationTypeId,
                command.TenantId
            );

            // Assign SecurityUser
            user.AssignSecurityUser(SecurityUser.Create(
                await passwordHasher.HashPassword(command.Password)
            ));

            // Add the user to the repository
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



