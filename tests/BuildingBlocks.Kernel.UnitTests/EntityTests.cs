using BuildingBlocks.Kernel.Domain;
using FluentAssertions;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class EntityTests
{
    private sealed class Customer : Entity<Guid>
    {
        public Customer(Guid id) : base(id) { }
    }

    private sealed class Product : Entity<Guid>
    {
        public Product(Guid id) : base(id) { }
    }

    [Fact]
    public void Entities_with_same_id_and_type_are_equal()
    {
        var id = Guid.NewGuid();
        var a = new Customer(id);
        var b = new Customer(id);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        new Customer(Guid.NewGuid()).Should().NotBe(new Customer(Guid.NewGuid()));
    }

    [Fact]
    public void Entities_of_different_types_with_same_id_are_not_equal()
    {
        var id = Guid.NewGuid();
        ((object)new Customer(id)).Should().NotBe(new Product(id));
    }
}
