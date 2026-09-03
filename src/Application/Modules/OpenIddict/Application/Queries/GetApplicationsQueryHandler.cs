// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Openddict.Queries;

public class GetApplicationsQueryHandler(
    ILogger<GetApplicationsQueryHandler> logger, 
    IOpenIddictApplicationManager applicationManager
) : IQueryHandler<GetApplicationsQuery, GetApplicationsPagedResponse>
{
    public async Task<GetApplicationsPagedResponse> HandleAsync(GetApplicationsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            List<OpenIddictApplicationDescriptor> applications = [];

            await foreach (var application in applicationManager.ListAsync(cancellationToken: cancellationToken))
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = await applicationManager.GetClientIdAsync(application, cancellationToken),
                    DisplayName = await applicationManager.GetDisplayNameAsync(application, cancellationToken),
                    ClientType = await applicationManager.GetClientTypeAsync(application, cancellationToken),
                    ApplicationType = await applicationManager.GetApplicationTypeAsync(application, cancellationToken),
                };

                foreach (var permission in await applicationManager.GetPermissionsAsync(application, cancellationToken))
                {
                    descriptor.Permissions.Add(permission);
                }

                foreach(var redirectUri in await  applicationManager.GetRedirectUrisAsync(application, cancellationToken))
                {
                    descriptor.RedirectUris.Add(new Uri(redirectUri));
                }

                applications.Add(descriptor);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();
                applications = applications.Where(a =>
                    (a.ClientId != null && a.ClientId.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (a.DisplayName != null && a.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var totalCount = applications.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedApplications = applications.Skip(skip).Take(query.PageSize).ToList();

            logger.LogInformation("Retrieved {Count} applications successfully.", pagedApplications.Count);
            return new GetApplicationsPagedResponse
            {
                Data = pagedApplications,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving applications.");
            throw;
        }
    }
}