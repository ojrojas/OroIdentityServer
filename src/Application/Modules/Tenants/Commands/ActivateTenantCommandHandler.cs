// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class ActivateTenantCommandHandler(
    ILogger<ActivateTenantCommandHandler> logger,
    ITenantRepository tenantRepository,
    IEventBus eventBus)
    : ICommandHandler<ActivateTenantCommand>
{
    public async Task HandleAsync(ActivateTenantCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling ActivateTenantCommand for TenantId: {TenantId}", command.TenantId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(TenantId.From(command.TenantId), ct)
                ?? throw new InvalidOperationException($"Tenant '{command.TenantId}' not found.");

            tenant.Activate();

            await tenantRepository.UpdateAsync(tenant, ct);

            await eventBus.PublishAsync(
                new TenantActivatedIntegrationEvent(tenant.Id.Value), ct);

            logger.LogInformation("Successfully activated tenant {TenantId}", command.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating tenant {TenantId}", command.TenantId);
            throw;
        }
    }
}
