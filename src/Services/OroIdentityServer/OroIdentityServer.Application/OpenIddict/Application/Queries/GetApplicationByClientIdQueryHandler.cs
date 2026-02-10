// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
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

            // Build descriptor using helper that parallelizes manager calls and validates URIs
            var descriptor = await BuildDescriptorAsync(application, query.ClientId, cancellationToken);

            logger.LogInformation("Application with ClientId {ClientId} retrieved successfully.", query.ClientId);
            return descriptor;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the application with ClientId {ClientId}.", query.ClientId);
            throw;
        }
    }

    private async Task<OpenIddictApplicationDescriptor> BuildDescriptorAsync(object application, string clientId, CancellationToken cancellationToken)
    {
        var displayNameTask = applicationManager.GetDisplayNameAsync(application, cancellationToken).AsTask();
        var permissionsTask = applicationManager.GetPermissionsAsync(application, cancellationToken).AsTask();
        var redirectUrisTask = applicationManager.GetRedirectUrisAsync(application, cancellationToken).AsTask();
        var postLogoutUrisTask = applicationManager.GetPostLogoutRedirectUrisAsync(application, cancellationToken).AsTask();
        var requirementsTask = applicationManager.GetRequirementsAsync(application, cancellationToken).AsTask();

        await Task.WhenAll(displayNameTask, permissionsTask, redirectUrisTask, postLogoutUrisTask, requirementsTask);

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = await displayNameTask
        };

        foreach (var permission in await permissionsTask)
        {
            descriptor.Permissions.Add(permission);
        }

        foreach (var uriStr in await redirectUrisTask)
        {
            if (Uri.TryCreate(uriStr, UriKind.Absolute, out var uri))
                descriptor.RedirectUris.Add(uri);
            else
                logger.LogWarning("Invalid redirect URI '{Uri}' for client {ClientId}.", uriStr, clientId);
        }

        foreach (var uriStr in await postLogoutUrisTask)
        {
            if (Uri.TryCreate(uriStr, UriKind.Absolute, out var uri))
                descriptor.PostLogoutRedirectUris.Add(uri);
            else
                logger.LogWarning("Invalid post-logout redirect URI '{Uri}' for client {ClientId}.", uriStr, clientId);
        }

        foreach (var requirement in await requirementsTask)
        {
            descriptor.Requirements.Add(requirement);
        }

        return descriptor;
    }
}