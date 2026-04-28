// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Queries;

public sealed class GetTenantsQueryHandler(
    ILogger<GetTenantsQueryHandler> logger,
    ITenantRepository tenantRepository)
    : IQueryHandler<GetTenantsQuery, GetTenantsResponse>
{
    public async Task<GetTenantsResponse> HandleAsync(GetTenantsQuery query, CancellationToken ct)
    {
        logger.LogInformation("Handling GetTenantsQuery");

        try
        {
            var tenants = await tenantRepository.GetAllAsync(ct);

            var dtos = tenants.Select(t => new TenantDto(
                t.Id.Value,
                t.Name.Value,
                t.Slug.Value,
                t.IsActive,
                t.CreatedAtUtc,
                t.TenantUsers.Count));

            logger.LogInformation("Successfully retrieved tenants");

            return new GetTenantsResponse
            {
                Data = dtos,
                Message = "Tenants retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenants");

            return new GetTenantsResponse
            {
                StatusCode = 500,
                Message = "An error occurred while retrieving tenants.",
                Errors = ["An error occurred while retrieving tenants."]
            };
        }
    }
}
