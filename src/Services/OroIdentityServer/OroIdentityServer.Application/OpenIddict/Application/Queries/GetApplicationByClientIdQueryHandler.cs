// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetApplicationByClientIdQueryHandler(
    ILogger<GetApplicationByClientIdQueryHandler> logger,
    IOpenIddictApplicationManager applicationManager
) : IQueryHandler<GetApplicationByClientIdQuery, OpenIddictApplicationDescriptor>
{
    public async Task<OpenIddictApplicationDescriptor> HandleAsync(GetApplicationByClientIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query.ClientId))
            {
                logger.LogError("ClientId is null or empty. Cannot retrieve application.");
                throw new ArgumentException("ClientId cannot be null or empty.", nameof(query.ClientId));
            }

            // Find the application by ClientId
            var application = await applicationManager.FindByClientIdAsync(query.ClientId, cancellationToken);

            if (application == null)
            {
                logger.LogWarning("Application with ClientId {ClientId} not found.", query.ClientId);
                throw new InvalidOperationException($"Application with ClientId {query.ClientId} not found.");
            }

            // Map the application to OpenIddictApplicationDescriptor
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = query.ClientId,
                DisplayName = await applicationManager.GetDisplayNameAsync(application, cancellationToken)
            };

            // Add permissions to the descriptor
            foreach (var permission in await applicationManager.GetPermissionsAsync(application, cancellationToken))
            {
                descriptor.Permissions.Add(permission);
            }

            logger.LogInformation("Application with ClientId {ClientId} retrieved successfully.", query.ClientId);
            return descriptor;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the application with ClientId {ClientId}.", query.ClientId);
            throw;
        }
    }
}