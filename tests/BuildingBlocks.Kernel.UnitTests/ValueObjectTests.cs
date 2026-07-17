using BuildingBlocks.Kernel.Domain;
using FluentAssertions;

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
        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Different_components_break_equality()
    {
        new Money(10, "USD").Should().NotBe(new Money(10, "EUR"));
        new Money(10, "USD").Should().NotBe(new Money(11, "USD"));
    }
}
