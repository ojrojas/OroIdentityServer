// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class ListTenantsQueryHandler(ILogger<ListTenantsQueryHandler> logger, ITenantRepository tenantRepository) : IQueryHandler<ListTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly ILogger<ListTenantsQueryHandler> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<IEnumerable<TenantDto>> HandleAsync(ListTenantsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ListTenantsQuery");
        var tenants = await _tenantRepository.GetAllTenantsAsync(cancellationToken);
        return tenants.Select(t => new TenantDto(t.Id, t.Name.Value, t.IsActive, t.CreatedAtUtc));
    }
}
