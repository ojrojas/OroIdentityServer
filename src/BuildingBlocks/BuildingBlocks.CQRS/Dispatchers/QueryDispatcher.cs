using System.Collections.Concurrent;
using BuildingBlocks.CQRS.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.Dispatchers;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private static readonly ConcurrentDictionary<(Type, Type), object> Cache = new();
    private readonly IServiceProvider _provider;

    public QueryDispatcher(IServiceProvider provider) => _provider = provider;

    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var queryType = query.GetType();
        var invoker = (Func<IServiceProvider, object, CancellationToken, Task<TResponse>>)Cache.GetOrAdd(
            (queryType, typeof(TResponse)), _ => BuildInvoker<TResponse>(queryType));
        return invoker(_provider, query, cancellationToken);
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<TResponse>> BuildInvoker<TResponse>(Type queryType)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(queryType, typeof(TResponse));
        var handleMethod = handlerType.GetMethod("HandleAsync")!;
        var behaviorMethod = behaviorType.GetMethod("HandleAsync")!;

        return (sp, q, ct) =>
        {
            var handler = sp.GetRequiredService(handlerType);
            Task<TResponse> Terminal(CancellationToken token) =>
                (Task<TResponse>)handleMethod.Invoke(handler, new object[] { q, token })!;

            var behaviors = sp.GetServices(behaviorType).Cast<object>().Reverse().ToArray();
            RequestHandlerDelegate<TResponse> next = Terminal;
            foreach (var behavior in behaviors)
            {
                var current = next;
                next = ct2 => (Task<TResponse>)behaviorMethod.Invoke(behavior, new object[] { q, current, ct2 })!;
            }
            return next(ct);
        };
    }
}
