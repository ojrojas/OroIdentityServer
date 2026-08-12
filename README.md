# OroIdentityServer

OroIdentityServer is an identity and authentication management system built on **.NET 10** and **ASP.NET Core**, implementing Domain-Driven Design (DDD) and Clean Architecture. It exposes an OAuth2 / OpenID Connect server via **OpenIddict**, a Blazor-based admin UI, a REST admin API, and an event-driven backbone over **RabbitMQ**.

## Key Features

### 1. Authentication and Authorization
- OAuth2 / OpenID Connect via OpenIddict 8 (authorization code, client credentials, password and refresh token flows)
- JWT issuance and validation, token revocation and authorization termination
- Cookie-based admin sign-in with shared DataProtection keyring
- Custom login/logout endpoints (`/auth/login`, `/auth/logout`, `/auth/change-password`)
- Login form rejects invalid credentials in place and shows an error, instead of silently redirecting
- Forced password change on first login for every user except the seeded admin account, enforced via a `must_change_password` claim and a redirect middleware that locks the UI to `/Account/ChangePassword` until cleared
- Relying-party-initiated logout (`~/connect/logout`) shows an IdentityServer-owned confirmation page (`/Account/Logout`) before ending the session, so signing out of a client app doesn't silently sign the admin out of IdentityServer itself

### 2. Blazor Admin UI
- FluentUI Blazor admin panel (list, detail/edit, create dialog, delete-with-confirmation) for Users, Roles, Applications, Scopes and Identification Types
- Sessions page to inspect and forcibly disconnect a user's active sessions
- Dashboard with live counts (users, connected users, roles, applications, scopes)
- Dark/light theme toggle (`FluentDesignTheme`) plus toast (`IToastService`) and dialog (`IDialogService`) providers for feedback and confirmations
- Runs under Blazor **Auto** render mode: components always call the `/api/*` endpoints over `HttpClient` (`IAdminXxxService`/`AdminXxxService` in `IdentityServer.Client`), whether rendered server-side on first paint or later from WebAssembly; the endpoints themselves delegate to CQRS-backed `ServerAdminXxxService` implementations (`IdentityServer/Services`) that talk to the dispatcher directly — no HTTP hop, no duplicated business logic between the two

### 3. Localization
- 8 supported languages: English, Spanish (LatAm), French, Italian, German, Portuguese (Brazil), Japanese, Chinese (Simplified) — `SharedResources.*.resx` under `IdentityServer.Client/Resources`
- Default culture resolves from the browser's `Accept-Language` header (which reflects the OS locale on Windows, Linux and macOS); an explicit choice from the language picker overrides it via a persisted `.AspNetCore.Culture` cookie
- Culture stays in sync between the server-rendered first paint and the WebAssembly runtime that takes over afterward (`/culture/set` endpoint + a small JS interop bootstrap in the client's `Program.cs`)

### 4. User, Role, Permission and Tenant Management
- Full CRUD for users, roles, permissions, tenants, identification types, applications and scopes
- Role/permission-based authorization for the admin API
- Multi-tenant support: tenant activation/suspension and user-to-tenant assignment
- User session and login-session tracking, with session termination revoking OpenIddict tokens/authorizations

### 5. Domain-Driven Design
- Aggregate roots, entities and value objects in `BuildingBlocks.Kernel`
- `Result`/`Error` pattern instead of exceptions for expected failures
- Business rules and domain events, dispatched via `DomainEventDispatcher`
- Repository + specification pattern for querying aggregates

### 6. CQRS — no MediatR
- Hand-rolled `ICommand`/`IQuery` abstractions and dispatchers in `BuildingBlocks.CQRS`
- Pipeline behaviors for logging and FluentValidation-based request validation

### 7. Event-Driven Integration
- `BuildingBlocks.EventBus` defines integration events and an in-memory subscription registry
- `BuildingBlocks.EventBus.RabbitMQ` provides a RabbitMQ-backed `IEventBus` implementation with Polly-based retry

### 8. Modern .NET Stack
- .NET 10 / ASP.NET Core minimal APIs
- Entity Framework Core with PostgreSQL (Npgsql)
- Serilog structured logging (console, Seq)
- OpenTelemetry instrumentation and Quartz-backed OpenIddict cleanup jobs
- FluentUI Blazor components for the built-in admin UI (Interactive Server + WebAssembly render modes)

### 9. Local Orchestration with .NET Aspire
- `examples/AppHost` wires up Postgres (+ pgAdmin), Redis, RabbitMQ and the identity server for local development
- `examples/Frontends/oroidentity-admin` is a sample Angular admin frontend, run through Aspire's Node/pnpm integration

## Project Structure

```
OroIdentityServer/
├── src/
│   ├── Core/                                   # Domain models, aggregates, interfaces (OroIdentityServer.Core)
│   ├── Application/                             # CQRS commands/queries and handlers, module extensions
│   ├── Infraestructure/                          # EF Core DbContext, migrations, repositories, specifications
│   ├── IdentityServer/
│   │   ├── IdentityServer/                       # Host: minimal API endpoints, OpenIddict, Blazor Server admin UI
│   │   │   ├── Endpoints/                        # /api/* minimal API groups, delegate to Services/ServerAdminXxxService
│   │   │   ├── Services/                         # ServerAdminXxxService: CQRS-backed IAdminXxxService implementations
│   │   │   └── Components/Accounts/Pages/        # Login, Logout (confirmation), ChangePassword (static SSR pages)
│   │   └── IdentityServer.Client/                # Blazor WebAssembly client (Auto render mode)
│   │       ├── Interfaces/, Services/            # IAdminXxxService + HTTP-based AdminXxxService (used by components)
│   │       ├── Models/                           # Client-facing request/response DTOs, per admin domain
│   │       ├── Pages/, Components/               # Admin CRUD pages, create dialogs, Dashboard, Sessions
│   │       └── Resources/                        # SharedResources.*.resx — localization (8 languages)
│   ├── Shared/
│   │   └── OroIdentityServer.Shared/             # Shared contracts across host/client
│   └── BuildingBlocks/
│       ├── BuildingBlocks.Kernel/                # DDD primitives: Entity, AggregateRoot, ValueObject, Result/Error
│       ├── BuildingBlocks.CQRS/                  # Custom command/query dispatchers and pipeline behaviors
│       ├── BuildingBlocks.EventBus/               # Integration event contracts and subscription registry
│       ├── BuildingBlocks.EventBus.RabbitMQ/       # RabbitMQ transport for the event bus
│       ├── BuildingBlocks.Logger/                 # Serilog configuration and logging extensions
│       └── BuildingBlocks.ServicesDefaults/        # Shared service registration helpers
├── examples/
│   ├── AppHost/                                  # .NET Aspire orchestration (Postgres, Redis, RabbitMQ, admin UI)
│   └── Frontends/oroidentity-admin/               # Sample Angular admin frontend
├── tests/
│   ├── Server.Tests/                             # Endpoint + authorization integration tests (WebApplicationFactory)
│   └── BuildingBlocks.*.UnitTests/                # Unit/integration tests per building block
├── data-protection-keys/                         # Shared DataProtection keyring for local development
├── docker-compose.yaml                           # Standalone PostgreSQL container (manual setup)
├── README.md
├── LICENCE
└── OroIdentityServer.slnx
```

## Technologies Used

- **.NET 10.0 / ASP.NET Core** — minimal APIs and Blazor (Interactive Server + WebAssembly)
- **OpenIddict 8** — OAuth2 / OpenID Connect server, validation and Quartz-based token cleanup
- **Entity Framework Core + Npgsql** — PostgreSQL persistence
- **FluentValidation** — request validation pipeline behavior
- **RabbitMQ.Client + Polly** — integration event bus with retry policies
- **Redis** — provisioned via Aspire for local development
- **.NET Aspire** — distributed application orchestration (`examples/AppHost`)
- **Serilog** — structured logging (console, Seq sink)
- **OpenTelemetry** — tracing/metrics instrumentation
- **Microsoft.FluentUI.AspNetCore.Components** — admin UI components (dark theme, toasts, dialogs, data grids)
- **Microsoft.Extensions.Localization** — RESX-based localization, 8 languages, satellite assemblies for WASM
- **Scalar.AspNetCore / Microsoft.OpenApi** — OpenAPI documentation
- **xUnit, NSubstitute, Testcontainers, EF Core InMemory** — test stack

## Domain-Driven Design Implementation

### Aggregate Roots
- `User`, `Role`, `Permission`, `Tenant`, `UserSession`, `IdentificationType` and related aggregates, one module per bounded context under `src/Core/Modules`

### Value Objects & Kernel Primitives
- `Entity`, `AggregateRoot`, `ValueObject`, `BusinessRule` in `BuildingBlocks.Kernel`
- `Result`/`Error` types for explicit, exception-free failure handling
- `IAuditableEntity` for created/modified auditing

### Repositories & Specifications
- Generic `IRepository<T>` and `ISpecification<T>` in the kernel
- Concrete repositories per aggregate under `src/Infraestructure/Repositories`

### CQRS (custom, no MediatR)
- `ICommand` / `IQuery` abstractions with dedicated `CommandDispatcher` / `QueryDispatcher`
- `LoggingBehavior` and `ValidationBehavior` pipeline behaviors

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker or Podman (for PostgreSQL, RabbitMQ, Redis)
- Node.js + pnpm (only if running the sample Angular admin frontend via Aspire)

### Local Development Setup (recommended: Aspire)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ojrojas/OroIdentityServer.git
   cd OroIdentityServer
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the AppHost:**
   ```bash
   dotnet run --project examples/AppHost/AppHost.csproj
   ```
   This provisions:
   - PostgreSQL (with pgAdmin) and a persistent data volume
   - RabbitMQ (persistent container lifetime)
   - Redis
   - The identity server (`identity-api`)
   - The sample Angular admin frontend (`identity-admin`, via pnpm)

4. **Access the applications** (ports are assigned by Aspire; check the dashboard for the current run):
   - **Aspire Dashboard:** printed in the console output when the AppHost starts (`https://localhost:17113` by default)
   - **Identity Server:** proxied through Aspire, or directly at the host's launch profile port
   - **Angular Admin Frontend:** proxied through Aspire on port `30645`

### Manual Setup (without Aspire)

1. **Start PostgreSQL:**
   ```bash
   docker compose up -d
   ```
   (uses `docker-compose.yaml`, database `identitydb` on port `5432`)

2. **Run database migrations:**
   ```bash
   dotnet ef database update --project src/Infraestructure/OroIdentityServer.Infraestructure.csproj --startup-project src/IdentityServer/IdentityServer/IdentityServer.csproj
   ```

3. **Run the identity server:**
   ```bash
   dotnet run --project src/IdentityServer/IdentityServer/IdentityServer.csproj
   ```
   Available at `http://localhost:5080` / `https://localhost:7114` (see `Properties/launchSettings.json`).

   > Note: without Aspire, RabbitMQ connection settings must be supplied manually if the event bus is enabled; otherwise disable/skip the RabbitMQ registration for local runs.

### Containerized Deployment (Podman / Docker)

The image is defined by `src/IdentityServer/IdentityServer/Dockerfile`. It exposes all runtime
settings as environment variables, so the same image can be used by anyone — on a laptop, on a
server, or in a cluster.

```bash
# Build the image
podman build -f src/IdentityServer/IdentityServer/Dockerfile -t oridentityserver:latest .

# Run it against a PostgreSQL instance
podman run --rm -p 5080:5080 \
  -e ConnectionStrings__identitydb="Host=db;Port=5432;Database=identitydb;Username=postgres;Password=Weak(!)Password123" \
  -e SEED_ADMIN_USERNAME="admin" \
  -e SEED_ADMIN_PASSWORD="Admin@123456" \
  oridentityserver:latest
```

Or use the provided compose file, which builds the image and wires it to a PostgreSQL container
(migrations and the universal `admin` seed run automatically on first start):

```bash
podman compose up -d --build
# Open http://localhost:5080 and sign in as `admin` / `Admin@123456`
```

The image always starts HTTP-only on port `5080`. To also serve HTTPS on port `5086`, mount a
certificate and override the URLs + certificate settings at run time (no application changes
needed). With HTTPS enabled, HTTP requests are redirected to HTTPS via `UseHttpsRedirection`:

```bash
podman run --rm -p 5080:5080 -p 5086:5086 \
  -v "$PWD/certs/https.pfx:/app/certs/https.pfx:ro" \
  -e ASPNETCORE_URLS="http://+:5080;https://+:5086" \
  -e Kestrel__Certificates__Default__Path="/app/certs/https.pfx" \
  -e Kestrel__Certificates__Default__Password="changeit" \
  -e ConnectionStrings__identitydb="Host=db;Port=5432;Database=identitydb;Username=postgres;Password=Weak(!)Password123" \
  -e SEED_ADMIN_USERNAME="admin" \
  -e SEED_ADMIN_PASSWORD="Admin@123456" \
  oridentityserver:latest
```

For a PEM certificate instead of a PFX, set `Kestrel__Certificates__Default__KeyPath` to the
private key file. Without a certificate, the container runs HTTP-only on port `5080`.

The universal seed creates an **Administrator** account with username `admin` (default password
`Admin@123456`). Change it before going live — see the `SEED_ADMIN_*` variables below.

## Configuration

Key configuration files:
- `src/IdentityServer/IdentityServer/appsettings.json` / `appsettings.Development.json` — logging, OpenIddict, DB connection
- `Directory.Build.props` — shared build properties
- `Directory.Packages.props` — centralized (central package management) NuGet versions
- `Data/seedData.json` (under the host project) — seed data for users, roles, applications and scopes on first run; controlled by the `DatabaseSeeder:Skip` setting. The universal bootstrap admin is the `admin` account (role **Administrator**, username `admin`, default password `Admin@123456`), which is exempt from the forced first-login password change; every other user (seeded or created later) must change their password on first sign-in. The admin identity can be customized through the `SEED_ADMIN_*` environment variables listed below.

#### Environment variables

The Dockerfile declares sensible defaults for every variable; each one can be overridden with
`-e` (Podman/Docker) or in the compose file:

| Variable | Default | Purpose |
| --- | --- | --- |
| `ASPNETCORE_URLS` | `http://+:5080` | URLs Kestrel binds to. Override to `http://+:5080;https://+:5086` (with a certificate) to also serve HTTPS |
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET environment |
| `Kestrel__Certificates__Default__Path` | *(unset)* | PFX (or PEM cert) path used for the HTTPS endpoint |
| `Kestrel__Certificates__Default__Password` | *(unset)* | Password of the PFX certificate |
| `Kestrel__Certificates__Default__KeyPath` | *(unset)* | Private key path for a PEM certificate |
| `ConnectionStrings__identitydb` | `Host=db;...` | PostgreSQL connection string |
| `SymmetricSecurityKey` | dev key | Base64 OpenIddict signing/encryption key (≥ 32 bytes) — **must be overridden in production** and shared by all instances |
| `IDENTITY_ADMIN_HTTP` | `http://localhost:4200` | Base URL of the external admin SPA, used when seeding the OpenIddict `Admin` client |
| `DatabaseSeeder__Skip` | `false` | Set `true` to skip the seeder on startup |
| `SEED_ADMIN_USERNAME` | `admin` | Username of the bootstrap admin |
| `SEED_ADMIN_PASSWORD` | `Admin@123456` | Password of the bootstrap admin |
| `SEED_ADMIN_EMAIL` | `admin@example.com` | Email of the bootstrap admin |
| `SEED_ADMIN_NAME` | `Admin` | First name of the bootstrap admin |
| `SEED_ADMIN_LASTNAME` | `Administrator` | Last name of the bootstrap admin |
| `SEED_ADMIN_IDENTIFICATION` | `000000001` | Identification number of the bootstrap admin |
| `SEED_ADMIN_ROLE` | `Administrator` | Role granted to the bootstrap admin |
| `SEED_ADMIN_FORCE_PASSWORD_CHANGE` | `false` | `true` forces the admin to change its password on first login |
| `EventBus__RabbitMQ__HostName` | `localhost` | RabbitMQ host (optional) |
| `EventBus__RabbitMQ__Port` | `5672` | RabbitMQ port (optional) |
| `EventBus__RabbitMQ__UserName` | `guest` | RabbitMQ user (optional) |
| `EventBus__RabbitMQ__Password` | `guest` | RabbitMQ password (optional) |
| `EventBus__RabbitMQ__VirtualHost` | `/` | RabbitMQ virtual host (optional) |

> A seed file mounted at `/app/Data/seedData.json` overrides the one baked into the image.

## API Endpoints

All admin endpoints are grouped under `/api` and generally require authentication/authorization; the `/auth` group is for sign-in/sign-out.

### Auth
- `POST /auth/login` — admin sign-in (invalid credentials redirect back to `/Account/Login` with an inline error)
- `POST /auth/logout` — admin sign-out (used by the panel's own header button, no confirmation step)
- `POST /auth/change-password` — sets a new password and clears the `must_change_password` claim
- `GET /Account/Login`, `/Account/ChangePassword`, `/Account/Logout` — the corresponding Blazor pages (static SSR)
- `GET/POST ~/connect/logout` — OpenIddict end-session endpoint; redirects to `/Account/Logout` for confirmation before signing out unless called with `confirmed=true`
- `GET /culture/set?culture={code}&redirectUri={uri}` — sets the `.AspNetCore.Culture` cookie and redirects back

### Users — `/api/users`
- `GET /` · `POST /` · `PUT /{id}` · `DELETE /{id}`

### Roles — `/api/roles`
- `GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `DELETE /{id}`

### Permissions — `/api/permissions`
- `GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `DELETE /{id}`

### Tenants — `/api/tenants`
- `GET /` · `GET /{id}` · `GET /by-user/{userId}` · `POST /`
- `PUT /{id}` · `POST /{id}/activate` · `POST /{id}/suspend` · `POST /{id}/users`

### Applications (OpenIddict clients) — `/api/applications`
- `GET /` · `GET /{clientId}` · `POST /` · `PUT /{clientId}` · `DELETE /{clientId}`

### Scopes — `/api/scopes`
- `GET /` · `POST /` · `PUT /{name}` · `DELETE /{name}`

### Identification Types — `/api/identification-types`
- `GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `DELETE /{id}`

### Sessions — `/api/sessions`
- `GET /by-user/{userId}` — login sessions for a user

### User Sessions — `/api/user-sessions`
- `GET /by-user/{userId}` · `GET /active-count` · `POST /` · `POST /{id}/deactivate`

## Testing

- `tests/Server.Tests` — integration tests against the host via `WebApplicationFactory`, covering auth endpoints and admin API authorization
- `tests/BuildingBlocks.Kernel.UnitTests` — kernel primitives (entities, results, business rules)
- `tests/BuildingBlocks.CQRS.UnitTests` — dispatcher and pipeline behavior tests
- `tests/BuildingBlocks.EventBus.UnitTests` — subscription registry tests
- `tests/BuildingBlocks.EventBus.RabbitMQ.IntegrationTests` — RabbitMQ event bus tests (Testcontainers)
- `tests/BuildingBlocks.Logger.UnitTests`, `tests/BuildingBlocks.ServicesDefaults.UnitTests`

Run all tests:
```bash
dotnet test
```

## License

Licensed under the GNU AGPL v3.0. See [LICENCE](./LICENCE) for details.
