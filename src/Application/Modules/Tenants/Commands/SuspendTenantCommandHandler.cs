// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class SuspendTenantCommandHandler(
    ILogger<SuspendTenantCommandHandler> logger,
    ITenantRepository tenantRepository,
    IEventBus eventBus)
    : ICommandHandler<SuspendTenantCommand>
{
    public async Task HandleAsync(SuspendTenantCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling SuspendTenantCommand for TenantId: {TenantId}", command.TenantId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(new(command.TenantId), ct)
                ?? throw new InvalidOperationException($"Tenant '{command.TenantId}' not found.");


            await tenantRepository.UpdateAsync(tenant, ct);

            await eventBus.PublishAsync(
                new TenantSuspendedIntegrationEvent(tenant.Id.Value), ct);

            logger.LogInformation("Successfully suspended tenant {TenantId}", command.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error suspending tenant {TenantId}", command.TenantId);
            throw;
        }
    }
}
