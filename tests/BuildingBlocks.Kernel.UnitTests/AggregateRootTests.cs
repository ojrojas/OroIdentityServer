using BuildingBlocks.Kernel.Domain;
using BuildingBlocks.Kernel.Events;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class AggregateRootTests
{
    private sealed record OrderCreated(Guid OrderId) : DomainEvent;

    private sealed class Order : AggregateRoot<Guid>
    {
        public Order(Guid id) : base(id) => RaiseDomainEvent(new OrderCreated(id));
        public void Touch() => RaiseDomainEvent(new OrderCreated(Id));
    }

    [Fact]
    public void Raising_domain_event_records_it()
    {
        var order = new Order(Guid.NewGuid());
        var domainEvent = Assert.Single(order.DomainEvents);
        Assert.IsType<OrderCreated>(domainEvent);
    }

    [Fact]
    public void Clearing_events_empties_collection()
    {
        var order = new Order(Guid.NewGuid());
        order.Touch();
        order.ClearDomainEvents();
        Assert.Empty(order.DomainEvents);
    }
}
