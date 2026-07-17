using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.DependencyInjection;
using BuildingBlocks.Kernel.Results;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.UnitTests;

public sealed class CommandDispatcherTests
{
    public sealed record CreateThing(string Name) : ICommand<Guid>;
    public sealed record DeleteThing(Guid Id) : ICommand;

    public sealed class CreateThingHandler : ICommandHandler<CreateThing, Guid>
    {
        public Task<Guid> HandleAsync(CreateThing command, CancellationToken ct) => Task.FromResult(Guid.NewGuid());
    }

    public sealed class DeleteThingHandler : ICommandHandler<DeleteThing>
    {
        public Task<Result> HandleAsync(DeleteThing command, CancellationToken ct) => Task.FromResult(Result.Success());
    }

    [Fact]
    public async Task Dispatches_typed_command_to_handler()
    {
        var sp = new ServiceCollection()
            .AddBuildingBlocksCqrs(typeof(CommandDispatcherTests).Assembly)
            .BuildServiceProvider();

        var dispatcher = sp.GetRequiredService<ICommandDispatcher>();
        var id = await dispatcher.SendAsync(new CreateThing("a"));
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Dispatches_void_command_to_handler()
    {
        var sp = new ServiceCollection()
            .AddBuildingBlocksCqrs(typeof(CommandDispatcherTests).Assembly)
            .BuildServiceProvider();

        var result = await sp.GetRequiredService<ICommandDispatcher>().SendAsync(new DeleteThing(Guid.NewGuid()));
        result.IsSuccess.Should().BeTrue();
    }
}
