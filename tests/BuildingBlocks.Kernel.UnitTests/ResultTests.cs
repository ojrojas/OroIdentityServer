using BuildingBlocks.Kernel.Results;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Success_carries_value()
    {
        Result<int> r = 42;
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
    }

    [Fact]
    public void Failure_blocks_value_access()
    {
        Result<int> r = Error.Validation("v", "bad");
        Assert.True(r.IsFailure);
        Action act = () => _ = r.Value;
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Match_routes_per_outcome()
    {
        Assert.Equal(8, Result<int>.Success(7).Match(v => v + 1, _ => 0));
        Assert.Equal(-1, Result.Failure<int>(Error.Validation("v", "x")).Match(v => v, _ => -1));
    }
}
