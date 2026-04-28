// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class UpdateTenantCommandHandler(
    ILogger<UpdateTenantCommandHandler> logger,
    ITenantRepository tenantRepository)
    : ICommandHandler<UpdateTenantCommand>
{
    public async Task HandleAsync(UpdateTenantCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling UpdateTenantCommand for TenantId: {TenantId}", command.TenantId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(TenantId.From(command.TenantId), ct)
                ?? throw new InvalidOperationException($"Tenant '{command.TenantId}' not found.");

            tenant.UpdateName(new TenantName(command.Name));

            await tenantRepository.UpdateAsync(tenant, ct);

            logger.LogInformation("Successfully updated tenant {TenantId}", command.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant {TenantId}", command.TenantId);
            throw;
        }
    }
}
