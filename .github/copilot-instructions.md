# Instrucciones para agentes AI — OroIdentityServer

Estas instrucciones están pensadas para agentes de codificación (Copilot, ChatGPT, etc.) que trabajan en este repositorio. Son concisas y prácticas, centradas en la arquitectura DDD usada en el proyecto.

```instructions
# Instructions for AI agents — OroIdentityServer

These instructions are for coding agents (Copilot, ChatGPT, etc.) working in this repository. They are concise, practical, and focused on the project's DDD architecture.

## Useful commands (quick)
- Restore packages: `dotnet restore`
- Build solution: `dotnet build OroIdentityServer.slnx`
- Run local environment (Aspire): `dotnet run --project src/AppHost/AppHost.csproj`
- EF migrations (when needed): `dotnet ef database update --project src/Services/OroIdentityServer/OroIdentityServer.Infraestructure`

## General style and conventions
- C# using modern .NET idioms (nullable enabled, expression-bodied members where appropriate).
- Prefer `record` for immutable DTOs/commands/queries.
- Namespaces should reflect layer and responsibility. Do NOT use a namespace that is identical to an entity, service or model name. Example: `OroIdentityServer.Application.Users.Commands` is correct — avoid `OroIdentityServer.Application.Users` as a namespace that collides with types.

## Architecture (high level)
- Main layers:
  - `src/Services/OroIdentityServer/OroIdentityServer.Core` — domain (entities, value objects, events, interfaces)
  - `src/Services/OroIdentityServer/OroIdentityServer.Application` — use cases (commands, queries, handlers)
  - `src/Services/OroIdentityServer/OroIdentityServer.Infraestructure` — persistence (DbContext, repositories, EF configurations)
  - `src/Frontends/*` — UI (Angular/Blazor) — frontend-specific rules in `src/Frontends/identity-admin/.github/copilot-instructions.md`

## OroIdentityServer.Core — concrete DDD rules
- Entities: declare aggregates/entities as classes inheriting from `BaseEntity<TId>` (see shared kernel under `src/Services/.../Shared/Entities`). Encapsulate invariants via instance methods and prefer immutable fields where possible.
- Value Objects: implement as `record` or immutable classes (derive from `BaseValueObject` or implement property-based equality). Perform validation and normalization in constructors or factory methods.
- Domain Events: define domain events as `record` types implementing `IDomainEvent` or inheriting `DomainEventBase`. Events should carry only the data necessary for subscribers; avoid referencing repositories or heavy logic inside events.
- Domain Services: when behavior crosses aggregate boundaries, create `IDomainService` interfaces and implementations in `Core/Services`. Aggregates should not depend on infrastructure concerns.
- Dependency injection in Core: expose extension methods in `src/Services/OroIdentityServer/OroIdentityServer.Core/Extensions` to register domain services (e.g. `AddDomainServices(this IServiceCollection services)`).

## OroIdentityServer.Infraestructure — persistence and repositories
- Repositories: implement concrete repositories that inherit from a generic base (e.g. `RepositoryBase<T>` or `EfRepository<T>`) and implement interfaces defined in `Core/Interfaces` (e.g. `IRepository<T>`, `IUserRepository`). Keep contracts in `Core` and implementations in `Infraestructure`.
- `DbContext`: create the EF context (e.g. `OroIdentityDbContext`) by inheriting from `AuditableDbContext` or `DbContext` from the shared kernel. Register `DbSet<TAggregate>` and apply model configurations via `OnModelCreating`.
- EF configurations: place `IEntityTypeConfiguration<T>` implementations under `Infraestructure/Data/Configurations` and register them with `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- Repository DI: expose extension methods in `src/Services/OroIdentityServer/OroIdentityServer.Infraestructure/Extensions` (e.g. `AddInfrastructure(this IServiceCollection services, IConfiguration cfg)`) to register the `DbContext` and repository implementations (`services.AddScoped<IUserRepository, UserRepository>()`).

## OroIdentityServer.Application — CQRS and handlers
- Commands/Queries: declare as `record` (immutable) and implement `IRequest<TResult>` (OroCQRS). Place them under `Commands`/`Queries` folders and use namespaces like `OroIdentityServer.Application.<Aggregate>.Commands`.
- Handlers: implement handlers adjacent to the command/query (e.g. `Handlers` folder) and implement `IRequestHandler<TRequest, TResult>`. Keep application orchestration, validation and repository calls inside handlers, using `Core` interfaces.
- Naming rules: avoid namespaces that are identical to type names. Use suffixes (`Commands`, `Handlers`, `Dtos`) to prevent collisions and ambiguity in DI/resolution.

## Quick examples (pattern snippets)
- Entity (Core): inherit `BaseEntity<UserId>`, validate in constructor, expose methods like `ChangeEmail()` that raise `UserEmailChanged` domain events.
- Value Object: `record Email(string Value)` with a factory `Email.Create(string)` that validates and normalizes.
- Domain Event: `record UserCreated(UserId Id, string Email) : IDomainEvent`.
- Command/Handler: `record CreateUserCommand(string Email) : IRequest<UserId>`; `class CreateUserHandler : IRequestHandler<CreateUserCommand, UserId> { ... }`.

## Integration points and notes
- OpenIddict and EF/Postgres are configured in `Infraestructure`; keep secrets out of the repository (use `appsettings.Development.json` only for local seed data).
- Logging is provided by Serilog configured from `AppHost` and `BuildingBlocks/Loggers`.

## Blazor Client-Server (shared service interface)
- The Blazor client (`src/Frontends/OroIdentity.Web/OroIdentity.Web.Client`) and the Server (`src/Frontends/OroIdentity.Web/OroIdentity.Web.Server`) are the same logical application with automatic global rendering. Pages live in the client project; the client communicates with the server over HTTP.
- Create a service interface in the client project and reference it from both projects. Recommended location: `src/Frontends/OroIdentity.Web/OroIdentity.Web.Client/Services/IClientAppService.cs`.
- Implementations:
  - Client implementation (HTTP caller): implement `IClientAppService` in the client project. Use `HttpClient` to call server endpoints (the server hosts the API endpoints under the server project). Example file: `src/Frontends/OroIdentity.Web/OroIdentity.Web.Client/Services/ClientAppService.cs`.
  - Server implementation (server side behavior): implement the same `IClientAppService` in the server project to run server-side logic or call external APIs. Example file: `src/Frontends/OroIdentity.Web/OroIdentity.Web.Server/Services/ServerAppService.cs`.
- DI and registration:
  - Client (Blazor): register the HTTP implementation in `Program.cs` (WASM or Server) as scoped:

```csharp
builder.Services.AddScoped<IClientAppService, ClientAppService>();
```

  - Server: register the server implementation in the server `Program.cs`:

```csharp
builder.Services.AddScoped<IClientAppService, ServerAppService>();
```

- Behavior contract:
  - The `IClientAppService` must expose methods for all calls the UI needs (e.g. `GetRolesAsync()`, `CreateApplicationAsync(...)`). The client-side implementation translates those methods into HTTP calls to server endpoints (e.g. `/api/roles`). The server-side implementation performs the actual orchestration (application services, repositories, external API calls).
  - Keep DTOs and request/response records in a shared library if needed (or in the `Application` layer) so both implementations use the same types.

- Examples in repository: follow patterns used for `Roles` and `Applications` pages in the client — client code issues HTTP requests; server endpoints perform domain/application work.

- Use local libraries: in web projects (client components and server) prefer the local building blocks `OroServiceDefaults` and `OroLoggers` for consistent service defaults and logging. Register and use them in `Program.cs` for web projects and `AppHost` where applicable.

## Security
- Do not commit `appsettings.*.Local` with credentials. Verify secrets before opening PRs.
- Domain events must not include sensitive data (e.g. passwords).

## Where to look (concrete locations)
- Core: src/Services/OroIdentityServer/OroIdentityServer.Core/
- Infraestructure: src/Services/OroIdentityServer/OroIdentityServer.Infraestructure/
- Application: src/Services/OroIdentityServer/OroIdentityServer.Application/
- Frontend Angular rules: src/Frontends/identity-admin/.github/copilot-instructions.md

## Artifact creation recipes (how to create project artifacts)
Below are step-by-step templates and conventions to create new artifacts in each layer. Follow these precisely so new code integrates with DI, EF and the CQRS handlers.

Core (Domain)
- Location: `src/Services/OroIdentityServer/OroIdentityServer.Core/`
- Entity: create a class that inherits `BaseEntity<TEntity, TId>` or `BaseEntity<TEntityId>` depending on shared kernel patterns. Place under `Models/<Aggregate>/`.
  - File example: `src/Services/OroIdentityServer/OroIdentityServer.Core/Models/Widget/Widget.cs`
  - Pattern:
    - class header: `public sealed class Widget : BaseEntity<Widget, WidgetId>, IAuditableEntity, IAggregateRoot`
    - Encapsulate behaviour via methods (e.g. `ChangeName(string)`) that validate and raise domain events.
    - Use constructor/factory to enforce invariants; avoid public setters for identity fields.
- Value Object: use `record` or sealed class deriving from `BaseValueObject`.
  - File example: `WidgetName.cs` with `public sealed class WidgetName : BaseValueObject` or `public record WidgetName(string Value) : BaseValueObject`.
  - Validate and normalize in factory: `public static WidgetName Create(string value)`.
- Domain Event: use `record` types implementing `IDomainEvent` or inheriting `DomainEventBase`.
  - Example: `record WidgetCreated(WidgetId Id, string Name) : IDomainEvent`.
- Domain Service: add `IDomainService` interface under `Core/Services` and implementation under same folder; register via `Core/Extensions/AddDomainServices`.
  - Keep services infrastructure-agnostic; inject only domain interfaces.

Infrastructure (Persistence)
- Location: `src/Services/OroIdentityServer/OroIdentityServer.Infraestructure/`
- Repository Interface: define in `Core/Interfaces` when generic contract is needed (e.g. `IUserRepository` in `Infraestructure/Interfaces` here). Prefer contracts in Core when domain-level.
- Repository Implementation: create class under `Infraestructure/Repositories/<Aggregate>/` implementing the contract.
  - Constructor should accept `IRepository<TEntity>` (generic repo), `DbContext` or other required infra services.
  - Use repository implementation to translate domain queries to EF queries and specifications.
  - Example file: `Infraestructure/Repositories/WidgetRepository/WidgetRepository.cs` implementing `IWidgetRepository`.
- DbContext: add your aggregate `DbSet<T>` to `OroIdentityAppContext` (or project DbContext) and implement entity configurations.
  - Register entity mappings using `IEntityTypeConfiguration<T>` in `Infraestructure/Data/EntityConfigurations`.
  - In `OnModelCreating`, call `modelBuilder.ApplyConfigurationsFromAssembly(typeof(OroIdentityAppContext).Assembly);`.
- EF Configurations: implement `IEntityTypeConfiguration<TEntity>` classes and configure owned types, indexes, relations, and constructor-binding for aggregates.

- DI (Infrastructure): provide an extension method `AddInfrastructure(this IServiceCollection services, IConfiguration cfg)` in `Infraestructure/Extensions/InfraestructureExtensions.cs`.
  - Register `DbContext`:

```csharp
services.AddDbContext<OroIdentityAppContext>(options => options.UseNpgsql(cfg.GetConnectionString("Default")));
```

  - Register repositories and generic repository:

```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<IWidgetRepository, WidgetRepository>();
```

Application (Use cases, CQRS)
- Location: `src/Services/OroIdentityServer/OroIdentityServer.Application/`
- Commands / Queries: create `record` types under `Commands/<Aggregate>` and `Queries/<Aggregate>`.
  - Example: `record CreateWidgetCommand(WidgetName Name) : IRequest<WidgetId>`.
- Handlers: implement adjacent to the command/query or under a `Handlers` folder.
  - Implement `IRequestHandler<TRequest, TResult>` (or project-specific `ICommandHandler/IQueryHandler`) and inject repository interfaces from `Core`.
  - Perform validation, mapping, raise domain events on aggregates, call repository methods, and persist via unit-of-work/repository.
  - Example handler file: `CreateWidgetCommandHandler.cs`.
- DTOs / Responses: define immutable response records in `Application/<Aggregate>/Dtos` or `Queries/*Response` records.
- Registration: expose `AddApplication(this IServiceCollection services)` extension to register MediatR handlers and any application services.

Frontend / Blazor client-server service interface (shared)
- Location for interface: `src/Frontends/OroIdentity.Web/OroIdentity.Web.Client/Services/IClientAppService.cs` (shared by both client and server projects).
- Client implementation (HTTP): `src/Frontends/OroIdentity.Web/OroIdentity.Web.Client/Services/ClientAppService.cs`.
  - Use `HttpClient` injected via DI and call server endpoints (e.g. `httpClient.GetFromJsonAsync<RolesResponse>("api/roles")`).
- Server implementation (server-side behavior): `src/Frontends/OroIdentity.Web/OroIdentity.Web.Server/Services/ServerAppService.cs`.
  - Implement same interface but call application services or repositories directly.
- Registration:
  - Client `Program.cs`: `builder.Services.AddScoped<IClientAppService, ClientAppService>();`
  - Server `Program.cs`: `builder.Services.AddScoped<IClientAppService, ServerAppService>();`

Use local building blocks in web projects
- Prefer local helpers `OroServiceDefaults` and `OroLoggers` for consistent defaults across Blazor client and server and `AppHost`.
  - Example: `builder.Services.AddOroServiceDefaults()` or similar extension provided by `BuildingBlocks/ServiceDefaults`.

Code templates (minimal)
- New Entity (Core/Models/Thing/Thing.cs):

```csharp
public sealed class Thing : BaseEntity<Thing, ThingId>, IAuditableEntity, IAggregateRoot
{
    private Thing(ThingId id, ThingName name) : base(id) { Name = name; }
    public ThingName Name { get; private set; }

    public static Thing Create(ThingName name) => new Thing(new ThingId(Guid.NewGuid()), name);

    public void ChangeName(ThingName newName) { /* validate */ Name = newName; /* add domain event */ }
}
```

- New Repository (Infraestructure/Repositories/ThingRepository/ThingRepository.cs):

```csharp
public class ThingRepository : IThingRepository
{
    private readonly IRepository<Thing> repository;
    public ThingRepository(IRepository<Thing> repository) { this.repository = repository; }

    public Task AddThingAsync(Thing t, CancellationToken ct) => repository.AddAsync(t, ct);
    // queries using repository.CurrentContext to write EF expressions
}
```

---
Follow these recipes when adding new aggregates or features; if you want, I can scaffold a concrete example (entity + config + repo + command + handler + client service) for one aggregate to use as a template. 

---
If you want, I can:
- Add or expand concrete code examples in `Core`/`Infraestructure`/`Application`.
- Run `dotnet build` and report build errors for the modified layer.

Please tell me if you want concrete examples (e.g. a complete `User` entity, its repository and a create handler).

```
