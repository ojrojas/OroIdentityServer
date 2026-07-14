// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroBuildingBlocks.EventBus.Abstractions;
using OroBuildingBlocks.EventBus.Events;

namespace OroIdentityServer.Server.Extensions;

/// <summary>
/// Fallback event bus used at design-time (EF migrations) and when RabbitMQ
/// is not configured. Tenant/User integration events are silently dropped.
/// </summary>
internal sealed class NoOpEventBus(ILogger<NoOpEventBus> logger) : IEventBus
{
    public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("NoOpEventBus dropped event {EventType}", integrationEvent.GetType().Name);
        return Task.CompletedTask;
    }
}
