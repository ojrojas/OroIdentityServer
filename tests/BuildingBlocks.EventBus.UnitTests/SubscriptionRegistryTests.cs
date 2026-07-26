using BuildingBlocks.EventBus.Subscriptions;

namespace BuildingBlocks.EventBus.UnitTests;

public sealed class SubscriptionRegistryTests
{
    public sealed record OrderPaid(Guid OrderId) : IntegrationEvent;
    public sealed class OrderPaidHandler : IIntegrationEventHandler<OrderPaid>
    {
        public Task HandleAsync(OrderPaid @event, CancellationToken ct = default) => Task.CompletedTask;
    }
    public sealed class AnotherHandler : IIntegrationEventHandler<OrderPaid>
    {
        public Task HandleAsync(OrderPaid @event, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public void Add_then_remove_clears_subscription_and_raises_event()
    {
        var registry = new InMemorySubscriptionRegistry();
        string? removed = null;
        registry.OnEventRemoved += (_, name) => removed = name;

        registry.Add<OrderPaid, OrderPaidHandler>();
        Assert.True(registry.HasSubscriptions(nameof(OrderPaid)));

        registry.Remove<OrderPaid, OrderPaidHandler>();
        Assert.False(registry.HasSubscriptions(nameof(OrderPaid)));
        Assert.Equal(nameof(OrderPaid), removed);
    }

    [Fact]
    public void Multiple_handlers_for_same_event_coexist()
    {
        var registry = new InMemorySubscriptionRegistry();
        registry.Add<OrderPaid, OrderPaidHandler>();
        registry.Add<OrderPaid, AnotherHandler>();
        Assert.Equal(2, registry.GetHandlersFor(nameof(OrderPaid)).Count());
    }

    [Fact]
    public void Adding_same_handler_twice_throws()
    {
        var registry = new InMemorySubscriptionRegistry();
        registry.Add<OrderPaid, OrderPaidHandler>();
        var act = () => registry.Add<OrderPaid, OrderPaidHandler>();
        Assert.Throws<InvalidOperationException>(act);
    }
}
