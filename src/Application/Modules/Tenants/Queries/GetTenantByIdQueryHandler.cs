// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Queries;

public sealed class GetTenantByIdQueryHandler(
    ILogger<GetTenantByIdQueryHandler> logger,
    ITenantRepository tenantRepository)
    : IQueryHandler<GetTenantByIdQuery, GetTenantByIdResponse>
{
    public async Task<GetTenantByIdResponse> HandleAsync(GetTenantByIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Handling GetTenantByIdQuery for TenantId: {TenantId}", query.TenantId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(TenantId.From(query.TenantId), ct);

            if (tenant is null)
            {
                return new GetTenantByIdResponse
                {
                    StatusCode = 404,
                    Message = $"Tenant '{query.TenantId}' not found.",
                    Errors = [$"Tenant with id '{query.TenantId}' was not found."]
                };
            }

            var dto = new TenantDetailDto(
                tenant.Id.Value,
                tenant.Name.Value,
                tenant.Slug.Value,
                tenant.IsActive,
                tenant.CreatedAtUtc,
                tenant.TenantUsers.Count,
                tenant.TenantUsers.Select(u => new TenantUserDto(
                    u.UserId.Value,
                    u.UserRoles.Select(r => r.RoleId.Value).ToList(),
                    u.IsActive,
                    u.JoinedAtUtc)).ToList(),
                null);

            logger.LogInformation("Successfully retrieved tenant {TenantId}", query.TenantId);

            return new GetTenantByIdResponse
            {
                Data = dto,
                Message = "Tenant retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenant {TenantId}", query.TenantId);

            return new GetTenantByIdResponse
            {
                StatusCode = 500,
                Message = "An error occurred while retrieving the tenant.",
                Errors = ["An error occurred while retrieving the tenant."]
            };
        }
    }
}
