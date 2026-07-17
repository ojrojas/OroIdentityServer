namespace BuildingBlocks.EventBus.Subscriptions;

public sealed class InMemorySubscriptionRegistry : ISubscriptionRegistry
{
    private readonly Dictionary<string, List<Subscription>> _handlers = new();
    private readonly Dictionary<string, Type> _eventTypes = new();
    private readonly object _gate = new();

    public bool IsEmpty
    {
        get { lock (_gate) { return _handlers.Count == 0; } }
    }

    public event EventHandler<string>? OnEventRemoved;

    public string GetEventKey<TEvent>() where TEvent : IntegrationEvent
    {
        var instanceName = typeof(TEvent).Name;
        return instanceName;
    }

    public void Add<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var key = GetEventKey<TEvent>();
        lock (_gate)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = new List<Subscription>();
                _handlers[key] = list;
                _eventTypes[key] = typeof(TEvent);
            }
            if (list.Any(s => s.HandlerType == typeof(THandler)))
                throw new InvalidOperationException($"Handler {typeof(THandler).FullName} already registered for {key}.");
            list.Add(new Subscription(typeof(THandler)));
        }
    }

    public void Remove<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var key = GetEventKey<TEvent>();
        lock (_gate)
        {
            if (!_handlers.TryGetValue(key, out var list)) return;
            var sub = list.FirstOrDefault(s => s.HandlerType == typeof(THandler));
            if (sub is null) return;
            list.Remove(sub);
            if (list.Count == 0)
            {
                _handlers.Remove(key);
                _eventTypes.Remove(key);
                OnEventRemoved?.Invoke(this, key);
            }
        }
    }

    public bool HasSubscriptions(string eventName)
    {
        lock (_gate) { return _handlers.ContainsKey(eventName); }
    }

    public Type? GetEventType(string eventName)
    {
        lock (_gate) { return _eventTypes.GetValueOrDefault(eventName); }
    }

    public IReadOnlyCollection<Subscription> GetHandlersFor(string eventName)
    {
        lock (_gate)
        {
            return _handlers.TryGetValue(eventName, out var list)
                ? list.ToArray()
                : Array.Empty<Subscription>();
        }
    }

    public void Clear()
    {
        lock (_gate) { _handlers.Clear(); _eventTypes.Clear(); }
    }
}
