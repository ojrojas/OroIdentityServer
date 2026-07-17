using BuildingBlocks.Kernel.DependencyInjection;
using BuildingBlocks.Kernel.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Kernel.UnitTests;

public sealed class DomainEventDispatcherTests
{
    public sealed record Pinged(string Source) : DomainEvent;

    public sealed class CountingHandler : IDomainEventHandler<Pinged>
    {
        public static int Count;
        public Task HandleAsync(Pinged domainEvent, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref Count);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Dispatcher_invokes_registered_handlers()
    {
        CountingHandler.Count = 0;
        var services = new ServiceCollection()
            .AddBuildingBlocksKernel(typeof(DomainEventDispatcherTests).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IDomainEventDispatcher>();
        await dispatcher.DispatchAsync(new[] { new Pinged("test") });

        CountingHandler.Count.Should().Be(1);
    }
}
