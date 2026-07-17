using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.DependencyInjection;
using BuildingBlocks.CQRS.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.UnitTests;

public sealed class PipelineBehaviorTests
{
    public sealed record Echo(string Text) : IQuery<string>;

    public sealed class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<string> HandleAsync(Echo q, CancellationToken ct) => Task.FromResult(q.Text);
    }

    public sealed class EchoValidator : IValidator<Echo>
    {
        public Task<IReadOnlyCollection<ValidationFailure>> ValidateAsync(Echo i, CancellationToken ct) =>
            Task.FromResult<IReadOnlyCollection<ValidationFailure>>(
                string.IsNullOrWhiteSpace(i.Text)
                    ? new[] { new ValidationFailure(nameof(Echo.Text), "Required") }
                    : Array.Empty<ValidationFailure>());
    }

    [Fact]
    public async Task Validation_behavior_throws_on_failures()
    {
        var sp = new ServiceCollection()
            .AddLogging()
            .AddBuildingBlocksCqrs(typeof(PipelineBehaviorTests).Assembly)
            .AddValidationBehavior()
            .BuildServiceProvider();

        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();
        var act = () => dispatcher.SendAsync(new Echo(""));
        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Logging_behavior_does_not_change_response()
    {
        var sp = new ServiceCollection()
            .AddLogging()
            .AddBuildingBlocksCqrs(typeof(PipelineBehaviorTests).Assembly)
            .AddLoggingBehavior()
            .BuildServiceProvider();

        var result = await sp.GetRequiredService<IQueryDispatcher>().SendAsync(new Echo("ping"));
        result.Should().Be("ping");
    }
}
