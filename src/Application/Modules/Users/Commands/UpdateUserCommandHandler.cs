// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Commands;

public class UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling UpdateUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            // Retrieve the existing user
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User not found with UserId: {UserId}", command.UserId);
                return new UpdateUserResponse { StatusCode = (int)HttpStatusCode.NotFound, Message = "User not found." };
            }

            // Update user details
            user.UpdateDetails(
                command.Name,
                command.MiddleName,
                command.LastName,
                command.UserName,
                command.Email,
                command.Identification,
                new(command.IdentificationTypeId),
                new(command.TenantId)
            );

            // Update password only when a new value is provided (not masked, not empty)
            const string masked = "**********";
            if (!string.IsNullOrWhiteSpace(command.Password) && command.Password != masked)
            {
                if (user.SecurityUser is null)
                    return new UpdateUserResponse { StatusCode = (int)HttpStatusCode.BadRequest, Message = "Security user not found." };

                var hashed = await passwordHasher.HashPassword(command.Password);
                user.SecurityUser.ChangePassword(hashed);
            }

            // Persist changes
            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully updated user with UserId: {UserId}", command.UserId);
            return new UpdateUserResponse { StatusCode = (int)HttpStatusCode.OK, Data = new() };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the user with UserId: {UserId}", command.UserId);
            throw;
        }
    }
}