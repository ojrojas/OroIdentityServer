// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class DeleteApplicationCommandHandler(
    ILogger<DeleteApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager
) : ICommandHandler<DeleteApplicationCommand>
{
    public async Task HandleAsync(DeleteApplicationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.ClientId))
            {
                logger.LogError("ClientId is null or empty. Cannot delete application.");
                throw new ArgumentException("ClientId cannot be null or empty.", nameof(command.ClientId));
            }

            // Find the existing application
            var existingApplication = await applicationManager.FindByClientIdAsync(
                command.ClientId, cancellationToken);

            if (existingApplication == null)
            {
                logger.LogWarning("Application with ClientId {ClientId} not found.", command.ClientId);
                return;
            }

            // Delete the application
            await applicationManager.DeleteAsync(existingApplication, cancellationToken);

            logger.LogInformation("Application with ClientId {ClientId} deleted successfully.", command.ClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the application with ClientId {ClientId}.", command.ClientId);
            throw;
        }
    }
}