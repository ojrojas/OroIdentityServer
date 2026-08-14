## 1. In-memory transport

- [x] 1.1 Add `InMemoryEventBus` to `BuildingBlocks.EventBus` implementing `IEventBus` (and `IAsyncDisposable` no-op) that uses `ISubscriptionRegistry` + `IServiceScopeFactory` to dispatch published events to registered `IIntegrationEventHandler<TEvent>` handlers in-process (mirroring `RabbitMQEventBus.OnMessageReceivedAsync`; no serialization, no connection, never throws connection errors).
- [x] 1.2 Implement `SubscribeAsync`/`UnsubscribeAsync` in `InMemoryEventBus` via `registry.Add`/`registry.Remove`.

## 2. Transport selection and DI

- [x] 2.1 Extract the shared `ISubscriptionRegistry` + `IIntegrationEventHandler<>` handler-scan registration in `RabbitMQServiceCollectionExtensions` into a private helper reused by all entry points.
- [x] 2.2 Add `AddEventBus(IServiceCollection, IConfiguration, Action<RabbitMQOptions>?, params Assembly[])` in `BuildingBlocks.EventBus.RabbitMQ.DependencyInjection` that reads `EventBus:Mode` (default `InMemory`): `RabbitMQ` delegates to `AddRabbitMQEventBus`, otherwise registers `IEventBus → InMemoryEventBus` (scoped).
- [x] 2.3 Keep `AddRabbitMQEventBus` backward compatible and internally using the shared helper.

## 3. Application wiring

- [x] 3.1 Switch `ApplicationExtensions.AddApplicationExtensions` from `AddRabbitMQEventBus` to `AddEventBus`.
- [x] 3.2 Update `examples/AppHost/AppHost.cs` to set `EventBus__Mode=RabbitMQ` for broker deployments (explicit opt-in).

## 4. Verification

- [x] 4.1 Add unit tests for `InMemoryEventBus`: publish dispatches to all registered handlers, publish with no handlers completes, subscribe/unsubscribe updates the registry.
- [x] 4.2 Verify the existing `BuildingBlocks.EventBus.RabbitMQ.IntegrationTests` still pass against `AddRabbitMQEventBus`.
- [x] 4.3 `dotnet build` the solution and run `BuildingBlocks.EventBus.UnitTests`; confirm no broker is needed for the in-memory path.
