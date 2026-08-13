## Context

See proposal.md. Current state shaping this design:

- `IEventBus` is only registered as `RabbitMQEventBus` (`RabbitMQServiceCollectionExtensions.cs:22`), scoped, alongside `IRabbitMQConnection`, `ISubscriptionRegistry` (`InMemorySubscriptionRegistry`), and scanned `IIntegrationEventHandler<TEvent>` implementations (same file, lines 24-36).
- `RabbitMQEventBus.PublishAsync` throws `InvalidOperationException` when `TryConnectAsync` fails (`RabbitMQEventBus.cs:42-43`), and its in-process dispatch path (`OnMessageReceivedAsync`, lines 144-175) resolves handlers via `IServiceScopeFactory` + `ISubscriptionRegistry` by event name.
- The running app never calls `SubscribeAsync`; published tenant events are dropped (unbound exchange). `ApplicationExtensions.AddApplicationExtensions` calls `AddRabbitMQEventBus` (`ApplicationExtensions.cs:15`).
- The AppHost provisions RabbitMQ and passes `EventBus__RabbitMQ__*` env vars (`AppHost.cs:5,36-39`).

## Goals / Non-Goals

**Goals:**
- Default to an in-memory `IEventBus` so the server runs with no broker.
- Keep RabbitMQ as an explicitly configured transport with unchanged publish/subscribe semantics.
- Share handler registration and dispatch mechanics between both transports.

**Non-Goals:**
- Adding a RabbitMQ consumer hosted service (out of scope; RabbitMQ mode remains publish-capable exactly as today).
- Changing the `IEventBus` contract or the integration event model.
- Supporting additional brokers (Kafka, etc.).

## Decisions

### D1: `InMemoryEventBus` mirrors the RabbitMQ dispatch path
Add `InMemoryEventBus` to `BuildingBlocks.EventBus` implementing `IEventBus` (+ `IAsyncDisposable` no-op). It depends on `ISubscriptionRegistry` and `IServiceScopeFactory`, and dispatches exactly like `RabbitMQEventBus.OnMessageReceivedAsync`: resolve the event type + handler types from the registry, resolve each handler from a per-publish scope, invoke `HandleAsync` via the `IIntegrationEventHandler<>` interface (reflection), no serialization (same process). `PublishAsync` never opens a connection and never throws a connection error.
- Rationale: reuses the registry/DI plumbing already proven by the RabbitMQ path; subscribing = `registry.Add`, unsubscribing = `registry.Remove`.
- Alternative considered: a strongly-typed delegate dictionary (`Dictionary<Type, List<Func<object, Task>>>`) — rejected because it would split subscription bookkeeping from `ISubscriptionRegistry` and diverge from the RabbitMQ path.

### D2: Transport selection via `EventBus:Mode`, selector in the RabbitMQ DI assembly
Add `AddEventBus(IServiceCollection, IConfiguration, Action<RabbitMQOptions>?, params Assembly[])` in `BuildingBlocks.EventBus.RabbitMQ.DependencyInjection` (the package that knows both transports). It:
1. Registers `ISubscriptionRegistry` + scans `IIntegrationEventHandler<>` (extract the handler-scan block from `AddRabbitMQEventBus` into a shared private helper).
2. Reads `EventBus:Mode` (default `InMemory`). If `RabbitMQ`, delegates to `AddRabbitMQEventBus` (options, `IRabbitMQConnection`, `IEventBus→RabbitMQEventBus`). Otherwise registers `IEventBus→InMemoryEventBus` (scoped).
`AddRabbitMQEventBus` is kept as the explicit RabbitMQ-only entry point and internally uses the same shared helper.
- Rationale: the RabbitMQ package is the transport provider; the selector there avoids an assembly dependency from `BuildingBlocks.EventBus` back to `RabbitMQ`. Mode default of `InMemory` satisfies "runs without a broker".
- Alternative considered: selector in `BuildingBlocks.EventBus` — rejected: it cannot reference the RabbitMQ transport.

### D3: Configuration surface
`EventBus:Mode` ∈ `InMemory` (default) | `RabbitMQ`. `EventBus:RabbitMQ:*` unchanged. `ApplicationExtensions` switches to `AddEventBus`. The AppHost sets `EventBus__Mode=RabbitMQ` for broker deployments so the compose path stays on RabbitMQ while local runs default to in-memory.
- Rationale: one switch, no duplication of the handler registration; explicit opt-in to the broker.

## Risks / Trade-offs

- **RabbitMQ mode still has no consumer at runtime** → Existing behavior (publish-only) is unchanged; in-memory is the default and runs handlers. Documented in the proposal; a consumer host is a separate follow-up.
- **In-memory dispatch is synchronous** → A handler failure propagates to the publisher. Mitigation: identical to the RabbitMQ handler execution semantics today (exceptions are logged/Nacked there; in-memory propagates), and handlers are the app's own tenant-provisioning handlers which are idempotent in intent. If needed later, wrapping in-process dispatch with try/catch+log keeps parity.
- **Handler re-scanning duplicated** → Mitigation: single shared helper registers registry + handlers, so both modes register exactly once.

## Migration Plan

1. Add `InMemoryEventBus` to `BuildingBlocks.EventBus`.
2. Refactor `RabbitMQServiceCollectionExtensions` (shared helper + `AddEventBus` selector); keep `AddRabbitMQEventBus` backward compatible.
3. Switch `ApplicationExtensions` to `AddEventBus`; update AppHost to set `EventBus__Mode=RabbitMQ`.
4. Tests: in-memory dispatch unit tests; existing RabbitMQ integration tests keep passing (they call `AddRabbitMQEventBus` directly).
- Rollback: revert `ApplicationExtensions` to `AddRabbitMQEventBus`; no schema changes.
