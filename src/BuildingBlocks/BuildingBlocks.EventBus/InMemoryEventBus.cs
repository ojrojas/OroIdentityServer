using BuildingBlocks.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.EventBus;

public sealed class InMemoryEventBus : IEventBus, IAsyncDisposable
{
    private readonly ISubscriptionRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(
        ISubscriptionRegistry registry,
        IServiceScopeFactory scopeFactory,
        ILogger<InMemoryEventBus> logger)
    {
        _registry = registry;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventName = _registry.GetEventKey<TEvent>();
        var eventType = _registry.GetEventType(eventName);
        if (eventType is null)
        {
            _logger.LogDebug("No registered type for event {EventName}; nothing to dispatch", eventName);
            return;
        }

        var handlers = _registry.GetHandlersFor(eventName);
        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers registered for event {EventName}", eventName);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        foreach (var subscription in handlers)
        {
            var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
            if (handler is null)
            {
                _logger.LogWarning("Handler {HandlerType} could not be resolved for event {EventName}", subscription.HandlerType, eventName);
                continue;
            }

            var concreteHandler = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            var method = concreteHandler.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync))!;
            await ((Task)method.Invoke(handler, new object[] { @event, cancellationToken })!).ConfigureAwait(false);
        }
    }

    public Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _registry.Add<TEvent, THandler>();
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _registry.Remove<TEvent, THandler>();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
