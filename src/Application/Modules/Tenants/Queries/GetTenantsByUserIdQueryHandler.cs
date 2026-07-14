// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Queries;

public sealed class GetTenantsByUserIdQueryHandler(
    ILogger<GetTenantsByUserIdQueryHandler> logger,
    ITenantRepository tenantRepository)
    : IQueryHandler<GetTenantsByUserIdQuery, GetTenantsByUserIdResponse>
{
    public async Task<GetTenantsByUserIdResponse> HandleAsync(GetTenantsByUserIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Handling GetTenantsByUserIdQuery for UserId: {UserId}", query.UserId);

        try
        {
            var tenants = await tenantRepository.GetByUserIdAsync(new(query.UserId), ct);

            var dtos = tenants.Select(t => new TenantDto(
                t.Id.Value,
                t.Name.Value,
                t.Slug.Value,
                t.IsActive,
                t.CreatedAtUtc,
                t.TenantUsers.Count));

            logger.LogInformation("Successfully retrieved tenants for user {UserId}", query.UserId);

            return new GetTenantsByUserIdResponse
            {
                Data = dtos,
                Message = "Tenants retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenants for user {UserId}", query.UserId);

            return new GetTenantsByUserIdResponse
            {
                StatusCode = 500,
                Message = "An error occurred while retrieving tenants.",
                Errors = ["An error occurred while retrieving tenants."]
            };
        }
    }
}
