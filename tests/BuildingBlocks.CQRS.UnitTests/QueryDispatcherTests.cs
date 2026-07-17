using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.UnitTests;

public sealed class QueryDispatcherTests
{
    public sealed record GetGreeting(string Name) : IQuery<string>;

    public sealed class GetGreetingHandler : IQueryHandler<GetGreeting, string>
    {
        public Task<string> HandleAsync(GetGreeting query, CancellationToken ct) => Task.FromResult($"Hello, {query.Name}");
    }

    [Fact]
    public async Task Dispatches_query_to_handler()
    {
        var sp = new ServiceCollection()
            .AddBuildingBlocksCqrs(typeof(QueryDispatcherTests).Assembly)
            .BuildServiceProvider();

        var result = await sp.GetRequiredService<IQueryDispatcher>().SendAsync(new GetGreeting("World"));
        result.Should().Be("Hello, World");
    }
}
