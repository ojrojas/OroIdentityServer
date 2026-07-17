using BuildingBlocks.Kernel.Results;
using FluentAssertions;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Success_carries_value()
    {
        Result<int> r = 42;
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_blocks_value_access()
    {
        Result<int> r = Error.Validation("v", "bad");
        r.IsFailure.Should().BeTrue();
        var act = () => r.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_routes_per_outcome()
    {
        Result<int>.Success(7).Match(v => v + 1, _ => 0).Should().Be(8);
        Result.Failure<int>(Error.Validation("v", "x")).Match(v => v, _ => -1).Should().Be(-1);
    }
}
