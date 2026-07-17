using System.Collections.Concurrent;
using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.Kernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.Dispatchers;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<Result>>> VoidCache = new();
    private static readonly ConcurrentDictionary<(Type, Type), object> TypedCache = new();

    private readonly IServiceProvider _provider;
    public CommandDispatcher(IServiceProvider provider) => _provider = provider;

    public Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var commandType = command.GetType();
        var invoker = VoidCache.GetOrAdd(commandType, BuildVoidInvoker);
        return invoker(_provider, command, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var commandType = command.GetType();
        var invoker = (Func<IServiceProvider, object, CancellationToken, Task<TResponse>>)TypedCache.GetOrAdd(
            (commandType, typeof(TResponse)), _ => BuildTypedInvoker<TResponse>(commandType));
        return invoker(_provider, command, cancellationToken);
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<Result>> BuildVoidInvoker(Type commandType)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(Result));
        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync))!;
        var behaviorMethod = behaviorType.GetMethod("HandleAsync")!;

        return (sp, cmd, ct) =>
        {
            var handler = sp.GetRequiredService(handlerType);
            Task<Result> Terminal(CancellationToken token) =>
                (Task<Result>)handleMethod.Invoke(handler, new object[] { cmd, token })!;

            var behaviors = sp.GetServices(behaviorType).Cast<object>().Reverse().ToArray();
            RequestHandlerDelegate<Result> next = Terminal;
            foreach (var behavior in behaviors)
            {
                var current = next;
                next = ct2 => (Task<Result>)behaviorMethod.Invoke(behavior, new object[] { cmd, current, ct2 })!;
            }
            return next(ct);
        };
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<TResponse>> BuildTypedInvoker<TResponse>(Type commandType)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(TResponse));
        var handleMethod = handlerType.GetMethod("HandleAsync")!;
        var behaviorMethod = behaviorType.GetMethod("HandleAsync")!;

        return (sp, cmd, ct) =>
        {
            var handler = sp.GetRequiredService(handlerType);
            Task<TResponse> Terminal(CancellationToken token) =>
                (Task<TResponse>)handleMethod.Invoke(handler, new object[] { cmd, token })!;

            var behaviors = sp.GetServices(behaviorType).Cast<object>().Reverse().ToArray();
            RequestHandlerDelegate<TResponse> next = Terminal;
            foreach (var behavior in behaviors)
            {
                var current = next;
                next = ct2 => (Task<TResponse>)behaviorMethod.Invoke(behavior, new object[] { cmd, current, ct2 })!;
            }
            return next(ct);
        };
    }
}
