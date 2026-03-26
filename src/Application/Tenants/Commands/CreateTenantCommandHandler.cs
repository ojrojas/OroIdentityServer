// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class CreateTenantCommandHandler(
    ILogger<CreateTenantCommandHandler> logger,
    ITenantRepository tenantRepository
    ) : ICommandHandler<CreateTenantCommand>
{
    private readonly ILogger<CreateTenantCommandHandler> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task HandleAsync(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateTenantCommand for Name: {Name}", command.Name);

        try
        {
            var tenant = Tenant.Create(command.Name.Value);
            await _tenantRepository.AddTenantAsync(tenant, cancellationToken);
            _logger.LogInformation("Successfully created tenant with Name: {Name}", command.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling CreateTenantCommand for Name: {Name}", command.Name);
            throw;
        }
    }
}
