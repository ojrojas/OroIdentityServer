// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetScopesQueryHandler(
    ILogger<GetScopesQueryHandler> logger,
    IOpenIddictScopeManager scopeManager
) : IQueryHandler<GetScopesQuery, IEnumerable<OpenIddictScopeDescriptor>>
{
    public async Task<IEnumerable<OpenIddictScopeDescriptor>> HandleAsync(GetScopesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var scopes = new List<OpenIddictScopeDescriptor>();

            await foreach (var scope in scopeManager.ListAsync(100, cancellationToken: cancellationToken))
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    Name = await scopeManager.GetNameAsync(scope, cancellationToken),
                };

                foreach (var res in await scopeManager.GetResourcesAsync(scope, cancellationToken))
                {
                    descriptor.Resources.Add(res);
                }

            }

            logger.LogInformation("Retrieved {Count} scopes successfully.", scopes.Count);
            return scopes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving scopes.");
            throw;
        }
    }
}