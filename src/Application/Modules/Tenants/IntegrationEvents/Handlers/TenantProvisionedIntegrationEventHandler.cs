// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.IntegrationEvents.Handlers;

/// <summary>
/// Handles TenantProvisionedIntegrationEvent by creating the tenant's PostgreSQL schema.
/// </summary>
public sealed class TenantProvisionedIntegrationEventHandler(
    ILogger<TenantProvisionedIntegrationEventHandler> logger,
    ITenantSchemaProvisioner schemaProvisioner)
    : IIntegrationEventHandler<TenantProvisionedIntegrationEvent>
{
    public async Task Handle(TenantProvisionedIntegrationEvent integrationEvent)
    {
        logger.LogInformation(
            "Provisioning tenant schema for Slug: '{Slug}', TenantId: {TenantId}",
            integrationEvent.Slug, integrationEvent.TenantId);

        try
        {
            await schemaProvisioner.ProvisionSchemaAsync(integrationEvent.Slug);

            logger.LogInformation(
                "Tenant schema provisioned successfully for Slug: '{Slug}', TenantId: {TenantId}",
                integrationEvent.Slug, integrationEvent.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to provision tenant schema for Slug: '{Slug}', TenantId: {TenantId}",
                integrationEvent.Slug, integrationEvent.TenantId);
            throw;
        }
    }
}
