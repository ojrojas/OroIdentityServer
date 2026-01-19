// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class CreateApplicationCommandHandler(
    ILogger<CreateApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager
) : ICommandHandler<CreateApplicationCommand>
{
    public async Task HandleAsync(CreateApplicationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.descriptor.ClientId))
            {
                logger.LogError("ClientId is null or empty. Cannot create application.");
                throw new ArgumentException("ClientId cannot be null or empty.", nameof(command.descriptor.ClientId));
            }

            // Check if the application already exists
            var existingApplication = await applicationManager.FindByClientIdAsync(
                command.descriptor.ClientId, cancellationToken);

            if (existingApplication != null)
            {
                logger.LogWarning("Application with ClientId {ClientId} already exists.", command.descriptor.ClientId);
                return;
            }

            // Create the application
            await applicationManager.CreateAsync(command.descriptor, cancellationToken);

            logger.LogInformation("Application with ClientId {ClientId} created successfully.", command.descriptor.ClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the application with ClientId {ClientId}.", command.descriptor.ClientId);
            throw;
        }
    }
}