// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IUserRepository userRepository
) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling UpdateUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            // Retrieve the existing user
            var user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Update user details
            user.UpdateDetails(
                command.Name,
                command.MiddleName,
                command.LastName,
                command.UserName,
                command.Email,
                command.Identification,
                command.IdentificationTypeId
            );

            // Persist changes
            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully updated user with UserId: {UserId}", command.UserId);
            return new UpdateUserResponse { StatusCode = (int)HttpStatusCode.OK, Data = user };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the user with UserId: {UserId}", command.UserId);
            throw;
        }
    }
}