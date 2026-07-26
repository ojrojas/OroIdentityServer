using BuildingBlocks.Kernel.Domain;

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

        Assert.Equal(b, a);
        Assert.True(a == b);
        Assert.Equal(b.GetHashCode(), a.GetHashCode());
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        Assert.NotEqual(new Customer(Guid.NewGuid()), new Customer(Guid.NewGuid()));
    }

    [Fact]
    public void Entities_of_different_types_with_same_id_are_not_equal()
    {
        var id = Guid.NewGuid();
        Assert.NotEqual((object)new Product(id), (object)new Customer(id));
    }
}
