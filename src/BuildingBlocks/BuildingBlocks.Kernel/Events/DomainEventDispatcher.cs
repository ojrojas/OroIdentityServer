using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Kernel.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();
    private readonly IServiceProvider _provider;

    public DomainEventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var @event in events)
        {
            var eventType = @event.GetType();
            var handlerType = HandlerTypeCache.GetOrAdd(eventType, t => typeof(IDomainEventHandler<>).MakeGenericType(t));
            var handlers = _provider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
                var task = (Task)method.Invoke(handler, new object[] { @event, cancellationToken })!;
                await task.ConfigureAwait(false);
            }
        }
    }
}
