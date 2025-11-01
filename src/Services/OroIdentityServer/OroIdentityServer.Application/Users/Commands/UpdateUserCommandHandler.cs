// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IUserRepository userRepository
) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling CreateUserCommand for UserName: {UserName}", command.UserName);

        UpdateUserResponse userResponse = new();

        try
        {
            // Hash the password
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Hashing password for UserName: {UserName}", command.UserName);
          
            // Create the User object
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Mapping User object for UserName: {UserName}", command.UserName);
            var user = new User
            {
                UserName = command.UserName,
                Email = command.Email,
                Name = command.Name,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                Identification = command.Identification,
                IdentificationTypeId = command.IdentificationTypeId,
            };

            // Add the user to the repository
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Update User to repository for UserName: {UserName}", command.UserName);

           await userRepository.UpdateUserAsync(user, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Successfully handled UpdateUserCommand for UserName: {UserName}", command.UserName);
            
           userResponse.Data = user;
           userResponse.Message = "Updated user successful";
            
           return userResponse;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "An error occurred while handling UpdateUserCommand for UserName: {UserName}", command.UserName);
            throw;
        }
    }
}