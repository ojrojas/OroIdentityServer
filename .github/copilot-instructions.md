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

## Security
- Do not commit `appsettings.*.Local` with credentials. Verify secrets before opening PRs.
- Domain events must not include sensitive data (e.g. passwords).

## Where to look (concrete locations)
- Core: src/Services/OroIdentityServer/OroIdentityServer.Core/
- Infraestructure: src/Services/OroIdentityServer/OroIdentityServer.Infraestructure/
- Application: src/Services/OroIdentityServer/OroIdentityServer.Application/
- Frontend Angular rules: src/Frontends/identity-admin/.github/copilot-instructions.md

---
If you want, I can:
- Add or expand concrete code examples in `Core`/`Infraestructure`/`Application`.
- Run `dotnet build` and report build errors for the modified layer.

Please tell me if you want concrete examples (e.g. a complete `User` entity, its repository and a create handler).

```
