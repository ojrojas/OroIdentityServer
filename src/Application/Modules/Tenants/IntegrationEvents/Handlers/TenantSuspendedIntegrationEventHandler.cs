// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.IntegrationEvents.Handlers;

/// <summary>
/// Handles TenantSuspendedIntegrationEvent — disables tenant access and notifies downstream modules.
/// </summary>
public sealed class TenantSuspendedIntegrationEventHandler(
    ILogger<TenantSuspendedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<TenantSuspendedIntegrationEvent>
{
    public Task Handle(TenantSuspendedIntegrationEvent integrationEvent)
    {
        logger.LogWarning(
            "Tenant suspended. TenantId: {TenantId}. Downstream modules should restrict access.",
            integrationEvent.TenantId);

        // Future: notify Notifications module, disable scheduled jobs, etc.
        return Task.CompletedTask;
    }
}
