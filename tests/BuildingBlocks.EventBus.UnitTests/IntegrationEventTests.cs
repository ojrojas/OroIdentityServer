using BuildingBlocks.EventBus;
using FluentAssertions;

namespace BuildingBlocks.EventBus.UnitTests;

public sealed class IntegrationEventTests
{
    public sealed record CustomerRegistered(string Email) : IntegrationEvent;

    [Fact]
    public void New_event_has_unique_id_and_timestamp()
    {
        var a = new CustomerRegistered("a@b.com");
        var b = new CustomerRegistered("a@b.com");
        a.EventId.Should().NotBe(b.EventId);
        a.OccurredOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        a.EventName.Should().Be(nameof(CustomerRegistered));
    }
}
