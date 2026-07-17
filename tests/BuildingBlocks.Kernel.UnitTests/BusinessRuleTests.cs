using BuildingBlocks.Kernel.Domain;
using BuildingBlocks.Kernel.Exceptions;
using FluentAssertions;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class BusinessRuleTests
{
    private sealed class MustBePositive(int value) : IBusinessRule
    {
        public bool IsBroken() => value <= 0;
        public string Message => "Value must be positive.";
    }

    [Fact]
    public void Broken_rule_throws_validation_exception()
    {
        var act = () => BusinessRule.Check(new MustBePositive(-1));
        act.Should().Throw<BusinessRuleValidationException>()
           .Which.BrokenRule.Should().BeOfType<MustBePositive>();
    }

    [Fact]
    public void Satisfied_rule_does_not_throw()
    {
        var act = () => BusinessRule.Check(new MustBePositive(1));
        act.Should().NotThrow();
    }
}
