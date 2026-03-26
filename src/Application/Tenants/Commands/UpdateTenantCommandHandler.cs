// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class UpdateTenantCommandHandler(
    ILogger<UpdateTenantCommandHandler> logger,
    ITenantRepository tenantRepository
    ) : ICommandHandler<UpdateTenantCommand>
{
    private readonly ILogger<UpdateTenantCommandHandler> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task HandleAsync(UpdateTenantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateTenantCommand for Id: {Id}", command.Id);

        var tenant = await _tenantRepository.GetTenantByIdAsync(command.Id, cancellationToken);
        if (tenant == null) throw new InvalidOperationException("Tenant not found.");

        tenant.UpdateName(command.Name);
        if (command.IsActive)
            tenant.Activate();
        else
            tenant.Deactivate();

        await _tenantRepository.UpdateTenantAsync(tenant, cancellationToken);
        _logger.LogInformation("Tenant updated successfully: {Id}", command.Id);
    }
}
