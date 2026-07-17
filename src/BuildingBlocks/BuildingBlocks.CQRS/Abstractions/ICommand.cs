using BuildingBlocks.Kernel.Results;

namespace BuildingBlocks.CQRS.Abstractions;

/// <summary>Marker for a fire-and-forget command returning a Result.</summary>
public interface ICommand : ICommand<Result> { }

/// <summary>Command that produces a typed response.</summary>
public interface ICommand<TResponse> { }

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandDispatcher
{
    Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
