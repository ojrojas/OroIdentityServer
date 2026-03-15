// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetTenantQueryHandler(ILogger<GetTenantQueryHandler> logger, ITenantRepository tenantRepository) : IQueryHandler<GetTenantQuery, TenantDto>
{
    private readonly ILogger<GetTenantQueryHandler> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<TenantDto> HandleAsync(GetTenantQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetTenantQuery for Id: {Id}", query.Id);
        var tenant = await _tenantRepository.GetTenantByIdAsync(query.Id, cancellationToken);
        if (tenant == null) return null!; // caller handles null

        return new TenantDto(tenant.Id, tenant.Name.Value, tenant.IsActive, tenant.CreatedAtUtc);
    }
}
