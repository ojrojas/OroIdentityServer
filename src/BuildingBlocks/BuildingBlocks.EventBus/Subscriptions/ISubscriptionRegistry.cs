namespace BuildingBlocks.EventBus.Subscriptions;

public interface ISubscriptionRegistry
{
    bool IsEmpty { get; }
    event EventHandler<string>? OnEventRemoved;

    void Add<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    void Remove<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    bool HasSubscriptions(string eventName);
    string GetEventKey<TEvent>() where TEvent : IntegrationEvent;
    Type? GetEventType(string eventName);
    IReadOnlyCollection<Subscription> GetHandlersFor(string eventName);
    void Clear();
}

public sealed record Subscription(Type HandlerType);
