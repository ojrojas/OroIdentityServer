// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetApplicationsQueryHandler(
    ILogger<GetApplicationsQueryHandler> logger, 
    IOpenIddictApplicationManager applicationManager
) : IQueryHandler<GetApplicationsQuery, IEnumerable<OpenIddictApplicationDescriptor>>
{
    public async Task<IEnumerable<OpenIddictApplicationDescriptor>> HandleAsync(GetApplicationsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var applications = new List<OpenIddictApplicationDescriptor>();

            await foreach (var application in applicationManager.ListAsync(cancellationToken: cancellationToken))
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = await applicationManager.GetClientIdAsync(application, cancellationToken),
                    DisplayName = await applicationManager.GetDisplayNameAsync(application, cancellationToken)
                };

                foreach (var permission in await applicationManager.GetPermissionsAsync(application, cancellationToken))
                {
                    descriptor.Permissions.Add(permission);
                }

                applications.Add(descriptor);
            }

            logger.LogInformation("Retrieved {Count} applications successfully.", applications.Count);
            return applications;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving applications.");
            throw;
        }
    }
}