using BuildingBlocks.Kernel.Domain;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }
        public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
        protected override IEnumerable<object?> GetEqualityComponents() { yield return Amount; yield return Currency; }
    }

    [Fact]
    public void Two_value_objects_with_same_components_are_equal()
    {
        var a = new Money(10, "USD");
        var b = new Money(10, "USD");
        Assert.Equal(b, a);
        Assert.True(a == b);
        Assert.Equal(b.GetHashCode(), a.GetHashCode());
    }

    [Fact]
    public void Different_components_break_equality()
    {
        Assert.NotEqual(new Money(10, "USD"), new Money(10, "EUR"));
        Assert.NotEqual(new Money(10, "USD"), new Money(11, "USD"));
    }
}
