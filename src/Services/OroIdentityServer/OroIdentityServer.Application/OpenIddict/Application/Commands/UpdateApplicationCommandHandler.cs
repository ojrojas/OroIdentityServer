// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Abstractions.Mappers;

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateApplicationCommandHandler(
    ILogger<UpdateApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager
) : ICommandHandler<UpdateApplicationCommand>
{
    public async Task HandleAsync(UpdateApplicationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.descriptor.ClientId))
            {
                logger.LogError("ClientId is null or empty. Cannot update application.");
                throw new ArgumentException("ClientId cannot be null or empty.", nameof(command.descriptor.ClientId));
            }

            // Find the existing application
            var existingApplication = await applicationManager.FindByClientIdAsync(
                command.descriptor.ClientId, cancellationToken);

            if (existingApplication == null)
            {
                logger.LogWarning("Application with ClientId {ClientId} not found.", command.descriptor.ClientId);
                return;
            }

            var descriptor = command.descriptor.ToOpenIddict();

            await applicationManager.UpdateAsync(existingApplication, descriptor, cancellationToken);

            logger.LogInformation("Application with ClientId {ClientId} updated successfully.", command.descriptor.ClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the application with ClientId {ClientId}.", command.descriptor.ClientId);
            throw;
        }
    }
}