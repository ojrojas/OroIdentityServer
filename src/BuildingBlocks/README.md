# BuildingBlocks

.NET 10 / .NET 11 building blocks for DDD + TDD + Vertical Slices applications.

## Layout

```
src/
  BuildingBlocks.Kernel/             # DDD primitives (Entity, AggregateRoot, ValueObject, DomainEvent, Result)
  BuildingBlocks.Logger/             # Serilog wiring (Console / File / Loki / Seq)
  BuildingBlocks.ServicesDefaults/   # Health checks + OpenTelemetry + ASP.NET wiring
  BuildingBlocks.EventBus/           # Integration Event abstractions
  BuildingBlocks.EventBus.RabbitMQ/  # RabbitMQ implementation of IEventBus
  BuildingBlocks.CQRS/               # Custom CQRS (no MediatR): dispatchers + pipeline behaviors

tests/
  BuildingBlocks.*.UnitTests/
  BuildingBlocks.EventBus.RabbitMQ.IntegrationTests/  # Testcontainers (Docker-gated)
  BuildingBlocks.Examples.OrdersApi.IntegrationTests/ # WebApplicationFactory end-to-end

examples/
  BuildingBlocks.Examples.OrdersApi/ # Vertical-slice minimal API using every library
```

## Build & test

```pwsh
dotnet build
dotnet test
```

The RabbitMQ integration test silently no-ops when Docker is unavailable.

## Quick wire-up in your service

```csharp
var builder = WebApplication.CreateBuilder(args);
var asm = typeof(Program).Assembly;

builder.AddBuildingBlocksLogging();
builder.AddBuildingBlocksDefaults();
builder.Services.AddBuildingBlocksKernel(asm);
builder.Services.AddBuildingBlocksCqrs(asm)
    .AddLoggingBehavior()
    .AddValidationBehavior();
builder.Services.AddRabbitMQEventBus(builder.Configuration, handlerAssemblies: asm);

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapOrdersModule();
app.Run();
```
