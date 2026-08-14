## Why

Today `IEventBus` is hard-wired to `RabbitMQEventBus` (`RabbitMQServiceCollectionExtensions.cs:22`). `PublishAsync` throws when the broker is unreachable (`RabbitMQEventBus.cs:42-43`), so the IdentityServer cannot run a working app without RabbitMQ. In addition, the running app never calls `SubscribeAsync`, so published tenant events are dropped into an unbound exchange and their handlers (e.g. tenant schema provisioning) never execute.

## What Changes

- Introduce an **in-memory transport** of `IEventBus` (`InMemoryEventBus` in `BuildingBlocks.EventBus`) that dispatches to registered `IIntegrationEventHandler<TEvent>` in-process, using the same `ISubscriptionRegistry` and handler DI registration.
- Add **transport selection via configuration**: `EventBus:Mode` = `RabbitMQ` | `InMemory`. Defaults to `InMemory` when not configured, so the server runs with no external broker. RabbitMQ remains fully supported when explicitly configured.
- Refactor handler registration so `AddRabbitMQEventBus` keeps registering the bus + handlers for RabbitMQ mode, and add a transport-agnostic registration path so the same handlers work in both modes.
- In `InMemory` mode the existing handlers (tenant provisioned/suspended/activated) execute in-process, which also fixes the current gap where nothing subscribes to RabbitMQ at runtime.

## Capabilities

### New Capabilities

- `event-bus/transport-selector`: The event bus SHALL run in memory (in-process dispatch) when RabbitMQ is not configured, and SHALL use the RabbitMQ transport when configured via `EventBus:Mode=RabbitMQ`. The server MUST start and function without a broker.

### Modified Capabilities

- None. No existing specs exist for the event bus; this is the first.

## Impact

- **BuildingBlocks.EventBus**: new `InMemoryEventBus`, `IEventBus` unchanged, transport registration extensions.
- **BuildingBlocks.EventBus.RabbitMQ**: `AddRabbitMQEventBus` retained as the RabbitMQ transport; handler scanning refactored to be shared.
- **Application**: `AddApplicationExtensions` uses the new selector (default in-memory); tenant handlers unchanged.
- **AppHost/examples**: RabbitMQ resource stays; the app becomes runnable without it.
- **Tests**: unit tests for `InMemoryEventBus` dispatch/subscribe/unsubscribe; integration tests verify RabbitMQ mode still works and that in-memory mode runs without a broker.
