namespace BuildingBlocks.EventBus.UnitTests;

public sealed class IntegrationEventTests
{
    public sealed record CustomerRegistered(string Email) : IntegrationEvent;

    [Fact]
    public void New_event_has_unique_id_and_timestamp()
    {
        var a = new CustomerRegistered("a@b.com");
        var b = new CustomerRegistered("a@b.com");
        Assert.NotEqual(b.EventId, a.EventId);
        Assert.True(Math.Abs((a.OccurredOn - DateTimeOffset.UtcNow).TotalSeconds) <= 5);
        Assert.Equal(nameof(CustomerRegistered), a.EventName);
    }
}
