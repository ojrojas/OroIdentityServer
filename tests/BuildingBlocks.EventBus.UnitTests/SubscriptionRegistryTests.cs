using BuildingBlocks.EventBus;
using BuildingBlocks.EventBus.Subscriptions;
using FluentAssertions;

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
        registry.HasSubscriptions(nameof(OrderPaid)).Should().BeTrue();

        registry.Remove<OrderPaid, OrderPaidHandler>();
        registry.HasSubscriptions(nameof(OrderPaid)).Should().BeFalse();
        removed.Should().Be(nameof(OrderPaid));
    }

    [Fact]
    public void Multiple_handlers_for_same_event_coexist()
    {
        var registry = new InMemorySubscriptionRegistry();
        registry.Add<OrderPaid, OrderPaidHandler>();
        registry.Add<OrderPaid, AnotherHandler>();
        registry.GetHandlersFor(nameof(OrderPaid)).Should().HaveCount(2);
    }

    [Fact]
    public void Adding_same_handler_twice_throws()
    {
        var registry = new InMemorySubscriptionRegistry();
        registry.Add<OrderPaid, OrderPaidHandler>();
        var act = () => registry.Add<OrderPaid, OrderPaidHandler>();
        act.Should().Throw<InvalidOperationException>();
    }
}
