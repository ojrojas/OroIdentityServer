## Purpose

Makes the integration event bus run without an external broker by selecting an in-process transport by configuration, while keeping RabbitMQ as a supported transport.

## ADDED Requirements

### Requirement: Transport selected by configuration

The event bus SHALL run in-memory (dispatching to registered handlers in-process) when `EventBus:Mode` is unset or set to `InMemory`, and SHALL use the RabbitMQ transport when `EventBus:Mode` is set to `RabbitMQ`.

#### Scenario: Mode not configured

- **WHEN** `EventBus:Mode` is absent from configuration
- **THEN** the event bus runs in-memory and does not attempt any connection to a message broker

#### Scenario: InMemory mode configured

- **WHEN** `EventBus:Mode=InMemory` is configured
- **THEN** the event bus runs in-memory and does not require a broker

#### Scenario: RabbitMQ mode configured

- **WHEN** `EventBus:Mode=RabbitMQ` is configured
- **THEN** the event bus uses the RabbitMQ transport configured under `EventBus:RabbitMQ`

### Requirement: Server runs without a broker

The IdentityServer SHALL start and function normally when no message broker is available or configured; publishing an event in this mode SHALL NOT throw a connection error.

#### Scenario: No broker configured

- **WHEN** the server starts without any `EventBus:RabbitMQ` configuration
- **THEN** the server starts successfully and all non-event-bus functionality works

#### Scenario: Broker configured but unreachable in RabbitMQ mode

- **WHEN** `EventBus:Mode=RabbitMQ` and the broker is unreachable at publish time
- **THEN** publishing fails with a connection error (existing behavior is preserved for explicit RabbitMQ mode)

### Requirement: In-memory dispatch invokes handlers

In in-memory mode, publishing an integration event SHALL synchronously invoke every registered handler for that event in-process, using the same handler registrations as the RabbitMQ transport.

#### Scenario: Published event has registered handlers

- **WHEN** an integration event is published in in-memory mode and handlers are registered for it
- **THEN** each registered handler is invoked with the event and completes before publish returns

#### Scenario: Published event has no handlers

- **WHEN** an integration event is published in in-memory mode and no handler is registered for it
- **THEN** the publish completes without error and without invoking any handler

### Requirement: Handler registration independent of transport

Integration event handlers SHALL be registered the same way for both transports, so the same handler set works in in-memory and RabbitMQ mode.

#### Scenario: Handlers registered before transport selection

- **WHEN** handlers are registered and the transport is later selected as in-memory or RabbitMQ
- **THEN** the handlers are available to the selected transport without additional registration

### Requirement: RabbitMQ subscribe semantics preserved

In RabbitMQ mode, the existing publish/subscribe behavior SHALL be preserved: subscribing binds the queue to the exchange for the event's routing key and received messages are dispatched to the registered handlers.

#### Scenario: RabbitMQ subscribe and receive

- **WHEN** a handler is subscribed in RabbitMQ mode and a matching message arrives on the queue
- **THEN** the message is acknowledged and dispatched to the subscribed handler
