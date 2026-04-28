// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.IntegrationEvents.Handlers;

/// <summary>
/// Handles TenantActivatedIntegrationEvent — re-enables tenant access.
/// </summary>
public sealed class TenantActivatedIntegrationEventHandler(
    ILogger<TenantActivatedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<TenantActivatedIntegrationEvent>
{
    public Task Handle(TenantActivatedIntegrationEvent integrationEvent)
    {
        logger.LogInformation(
            "Tenant activated. TenantId: {TenantId}. Downstream modules should restore access.",
            integrationEvent.TenantId);

        // Future: re-enable scheduled jobs, send welcome-back notification, etc.
        return Task.CompletedTask;
    }
}
