using BuildingBlocks.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildingBlocks.EventBus.UnitTests;

public sealed class InMemoryEventBusTests
{
    public sealed record OrderShipped(Guid OrderId) : IntegrationEvent;
    public sealed record OrderCancelled(Guid OrderId) : IntegrationEvent;

    public sealed class OrderShippedHandler : IIntegrationEventHandler<OrderShipped>
    {
        public List<OrderShipped> Received { get; } = [];
        public Task HandleAsync(OrderShipped @event, CancellationToken ct = default)
        {
            Received.Add(@event);
            return Task.CompletedTask;
        }
    }

    public sealed class OrderShippedAuditHandler : IIntegrationEventHandler<OrderShipped>
    {
        public List<OrderShipped> Received { get; } = [];
        public Task HandleAsync(OrderShipped @event, CancellationToken ct = default)
        {
            Received.Add(@event);
            return Task.CompletedTask;
        }
    }

    public sealed class OrderCancelledHandler : IIntegrationEventHandler<OrderCancelled>
    {
        public List<OrderCancelled> Received { get; } = [];
        public Task HandleAsync(OrderCancelled @event, CancellationToken ct = default)
        {
            Received.Add(@event);
            return Task.CompletedTask;
        }
    }

    private static (IEventBus Bus, IServiceProvider Provider) CreateBus(params object[] handlers)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISubscriptionRegistry, InMemorySubscriptionRegistry>();
        services.AddSingleton<ILogger<InMemoryEventBus>>(NullLogger<InMemoryEventBus>.Instance);
        foreach (var handler in handlers)
            services.AddSingleton(handler.GetType(), handler);
        services.AddScoped<IEventBus, InMemoryEventBus>();

        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IEventBus>(), provider);
    }

    [Fact]
    public async Task PublishAsync_dispatches_to_all_registered_handlers()
    {
        var handler = new OrderShippedHandler();
        var audit = new OrderShippedAuditHandler();
        var (bus, _) = CreateBus(handler, audit);

        await bus.SubscribeAsync<OrderShipped, OrderShippedHandler>();
        await bus.SubscribeAsync<OrderShipped, OrderShippedAuditHandler>();

        var @event = new OrderShipped(Guid.NewGuid());
        await bus.PublishAsync(@event);

        Assert.Single(handler.Received);
        Assert.Single(audit.Received);
        Assert.Equal(@event.EventId, handler.Received[0].EventId);
        Assert.Equal(@event.EventId, audit.Received[0].EventId);
    }

    [Fact]
    public async Task PublishAsync_with_no_handlers_completes()
    {
        var (bus, _) = CreateBus();

        await bus.PublishAsync(new OrderShipped(Guid.NewGuid()));
    }

    [Fact]
    public async Task PublishAsync_only_invokes_handlers_for_the_published_event()
    {
        var shipped = new OrderShippedHandler();
        var cancelled = new OrderCancelledHandler();
        var (bus, _) = CreateBus(shipped, cancelled);

        await bus.SubscribeAsync<OrderShipped, OrderShippedHandler>();
        await bus.SubscribeAsync<OrderCancelled, OrderCancelledHandler>();

        await bus.PublishAsync(new OrderShipped(Guid.NewGuid()));

        Assert.Single(shipped.Received);
        Assert.Empty(cancelled.Received);
    }

    [Fact]
    public async Task SubscribeAsync_then_UnsubscribeAsync_updates_the_registry()
    {
        var handler = new OrderShippedHandler();
        var (bus, provider) = CreateBus(handler);
        var registry = provider.GetRequiredService<ISubscriptionRegistry>();

        await bus.SubscribeAsync<OrderShipped, OrderShippedHandler>();
        Assert.True(registry.HasSubscriptions(nameof(OrderShipped)));

        await bus.UnsubscribeAsync<OrderShipped, OrderShippedHandler>();
        Assert.False(registry.HasSubscriptions(nameof(OrderShipped)));
    }
}
