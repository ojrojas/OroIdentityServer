# OroIdentityServer

OroIdentityServer is a modern identity and authentication management system built with ASP.NET Core, implementing Domain-Driven Design (DDD) principles. This project provides a robust identity server using OpenIddict for OAuth2 and OpenID Connect, with full support for user, role, application, and scope management.

## Key Features

### 1. **Authentication and Authorization**
- Complete OAuth2 and OpenID Connect implementation using OpenIddict
- Support for authorization code, implicit, and client credentials flows
- JWT token generation and validation
- Secure logout and token revocation

### 2. **User Management**
- Full CRUD operations for users with DDD aggregates
- Secure password hashing and verification
- User profile management with identification types
- Multi-tenant support with tenant entities

### 3. **Role-Based Access Control**
- Hierarchical role management with claims
- User-role assignments with many-to-many relationships
- Role claims for fine-grained permissions

### 4. **Application and Scope Management**
- Dynamic client application registration
- Scope definition and management
- Consent handling for delegated access

### 5. **Domain-Driven Design (DDD)**
- Clean Architecture with separated concerns
- Aggregate roots with domain events
- Value objects for type safety
- Repository pattern with specifications

### 6. **Modern .NET Stack**
- Built on .NET 10.0 with latest features
- Entity Framework Core with PostgreSQL
- CQRS with MediatR for command/query separation
- Serilog for structured logging

### 7. **Container Orchestration**
- Aspire for distributed application orchestration
- Docker containers for PostgreSQL, pgAdmin, and Seq
- Easy local development setup

## Project Structure

```
OroIdentityServer/
├── src/
│   ├── AppHost/                          # Aspire orchestration
│   ├── BuildingBlocks/
│   │   ├── Loggers/                      # Logging infrastructure
│   │   └── ServiceDefaults/              # Common service extensions
│   ├── Frontends/
│   │   └── OroIdentity.Web/              # Blazor WebAssembly frontend
│   └── Services/
│       ├── OroIdentityServer.Api/        # Minimal API endpoints
│       ├── OroIdentityServer.Application/# CQRS handlers
│       ├── OroIdentityServer.Core/       # Domain models and interfaces
│       └── OroIdentityServer.Infraestructure/ # EF Core and repositories
├── nupkgs/                               # Local NuGet packages
├── index.html                            # Development index
├── README.md
├── LICENSE
└── OroIdentityServer.slnx
```

## Technologies Used

- **.NET 10.0**: Latest .NET runtime with performance improvements
- **ASP.NET Core**: Web framework for APIs and hosting
- **OpenIddict**: Open-source OAuth2/OpenID Connect server
- **Entity Framework Core**: ORM with PostgreSQL via Npgsql
- **MediatR**: In-process messaging for CQRS
- **Aspire**: .NET distributed application framework
- **PostgreSQL**: Robust relational database
- **Serilog**: Structured logging
- **Docker**: Containerization for development

## Domain-Driven Design Implementation

The project fully embraces DDD principles with the following components:

### Aggregate Roots
- `User`, `Role`, `SecurityUser`, `Tenant` as aggregate roots
- Domain events for cross-aggregate communication
- Encapsulation of business rules within aggregates

### Value Objects
- `UserId`, `RoleId`, `SecurityUserId`, `TenantId` for identity
- `UserName`, `Email`, `RoleName`, `TenantName` for domain values
- `RoleClaimType`, `RoleClaimValue` for claims
- Owned types in EF Core for complex value objects

### Repositories and Specifications
- Generic `IRepository<T>` interface
- Specific repositories for each aggregate
- Specification pattern for complex queries

### CQRS Pattern
- Commands for write operations (create, update, delete)
- Queries for read operations
- Separate handlers with MediatR

### Shared Kernel (OroKernel.Shared)
- `BaseEntity<TId>` for auditable entities
- `BaseValueObject` for value objects
- `IAggregateRoot` interface
- `AuditableDbContext` for EF Core base context

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker and Docker Compose
- Podman (alternative to Docker)

### Local Development Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ojrojas/OroIdentityServer.git
   cd OroIdentityServer
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Start with Aspire (recommended):**
   ```bash
   dotnet run --project src/AppHost/AppHost.csproj
   ```
   This will start:
   - PostgreSQL database
   - pgAdmin for database management
   - Seq for log aggregation
   - The identity server API
   - Blazor frontend

4. **Access the applications:**
   - **Aspire Dashboard:** http://localhost:15888
   - **Identity Server API:** http://localhost:5000
   - **Blazor Frontend:** http://localhost:5001
   - **pgAdmin:** http://localhost:45027
   - **Seq Logs:** http://localhost:42963

### Manual Setup (without Aspire)

1. **Start PostgreSQL:**
   ```bash
   podman run -d --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:15
   ```

2. **Run database migrations:**
   ```bash
   dotnet ef database update --project src/Services/OroIdentityServer/OroIdentityServer.Infraestructure
   ```

3. **Run the API:**
   ```bash
   dotnet run --project src/Services/OroIdentityServer/OroIdentityServer.Api
   ```

## Configuration

Key configuration files:
- `appsettings.json`: Application settings
- `Directory.Build.props`: Shared build properties
- `Directory.Packages.props`: Centralized package versions

Database connection and OpenIddict settings are configured in `appsettings.json`.

## API Endpoints

### Authentication
- `POST /connect/token` - Token endpoint
- `GET /connect/authorize` - Authorization endpoint
- `POST /connect/logout` - Logout endpoint

### Users
- `POST /users/create` - Create user
- `GET /users` - List users
- `GET /users/{id}` - Get user by ID

### Roles
- `POST /roles/create` - Create role
- `GET /roles` - List roles

### Applications
- `POST /applications/create` - Register application
- `GET /applications` - List applications

## Database Schema

The system uses PostgreSQL with the following main tables:
- `Users` - User accounts
- `SecurityUsers` - Security-related user data
- `Roles` - Role definitions
- `RoleClaims` - Role permissions (owned by Roles)
- `UserRoles` - User-role assignments
- `IdentificationTypes` - User identification types
- `Tenants` - Multi-tenant support

OpenIddict tables for OAuth2/OpenID Connect:
- `OpenIddictApplications`
- `OpenIddictScopes`
- `OpenIddictTokens`
- `OpenIddictAuthorizations`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

Licensed under GNU AGPL v3.0. See [LICENSE](./LICENSE) for details.

## Recent Updates

### v1.0.0 (Latest)
- ✅ Full DDD implementation with OroKernel.Shared
- ✅ EF Core integration with owned types and value objects
- ✅ Aspire orchestration for local development
- ✅ PostgreSQL with Npgsql provider
- ✅ OpenIddict for identity management
- ✅ CQRS with MediatR
- ✅ Blazor WebAssembly frontend
- ✅ Comprehensive seed data setup
- ✅ Containerized development environment

For more details, check the [changelog](CHANGELOG.md) or issues.

## Endpoints

### Users
- **POST** `/users/create`: Creates a new user.
- **PUT** `/users/update`: Updates an existing user.
- **DELETE** `/users/delete/{id}`: Deletes a user by ID.
- **GET** `/users/`: Lists all users.

### Roles
- **POST** `/roles/create`: Creates a new role.
- **PUT** `/roles/update`: Updates an existing role.
- **DELETE** `/roles/delete/{id}`: Deletes a role by ID.
- **GET** `/roles/`: Lists all roles.

### Applications
- **POST** `/applications/create`: Creates a new application.
- **PUT** `/applications/update`: Updates an existing application.
- **DELETE** `/applications/delete/{clientId}`: Deletes an application by its ClientId.
- **GET** `/applications/`: Lists all applications.
- **GET** `/applications/{clientId}`: Retrieves a specific application.

### Scopes
- **POST** `/scopes/create`: Creates a new scope.
- **PUT** `/scopes/update`: Updates an existing scope.
- **DELETE** `/scopes/delete/{name}`: Deletes a scope by its name.
- **GET** `/scopes/`: Lists all scopes.

## Building Blocks

The project includes several reusable building blocks that encapsulate common functionalities:

### Loggers
- **Main Files**:
  - `LoggerPrinter.cs`: Custom implementation for logging.
  - `LoggersExtensions.cs`: Extension methods for configuring logging.
- **Purpose**: Provide a centralized and extensible logging infrastructure.

### ServiceDefaults
- **Main Files**:
  - `AsyncExtensions.cs`: Extension methods for asynchronous operations.
  - `ClaimsExtensions.cs`: Extension methods for handling claims.
  - `ConfigurationExtensions.cs`: Extension methods for service configuration.
  - `Extensions.cs`: General utility methods.
  - `GetDestination.cs`: Logic for determining specific destinations.
- **Purpose**: Provide default extensions and configurations for common services.

### Shared
- **Subfolders**:
  - **Data**: Contains `AuditableDbContext.cs` for handling auditable entities.
  - **Entities**: Defines base classes like `BaseEntity.cs` and `BaseValueObject.cs`.
  - **Enums**: Contains enumerations like `EntityBaseState.cs`.
  - **Interfaces**: Defines contracts like `IAggregateRoot.cs`, `IAuditableEntity.cs`, `IRepository.cs`.
  - **Options**: Defines configurations like `RoleInfo.cs` and `UserInfo.cs`.
  - **Services**: Implements services like `ClaimsUserInfoService.cs` and `IdentityClientService.cs`.
- **Purpose**: Provide shared entities, interfaces, and services across different modules.

## Domain-Driven Design (DDD) Updates

The project has been fully updated to follow Domain-Driven Design (DDD) principles with OroKernel.Shared integration:

### 1. **Aggregate Roots & Entities**
- `User`, `SecurityUser`, `Role`, `Tenant` as aggregate roots
- Domain events for business logic decoupling
- Auditable entities with `BaseEntity<TId>`

### 2. **Value Objects**
- Strongly-typed IDs: `UserId`, `RoleId`, `SecurityUserId`, `TenantId`
- Domain values: `UserName`, `Email`, `RoleName`, `TenantName`
- Owned types in EF Core: `RoleClaimType`, `RoleClaimValue`
- Proper equality and validation

### 3. **Repositories & Specifications**
- Generic `IRepository<T>` with `IAggregateRoot` constraint
- Specific repositories for domain operations
- EF Core configurations with owned types and indexes

### 4. **CQRS with MediatR**
- Commands for write operations with validation
- Queries for read operations
- Separate handlers with dependency injection

### 5. **EF Core Integration**
- PostgreSQL with Npgsql
- Owned types for complex value objects
- Constructor binding fixes for aggregates
- Global query filters and soft deletes

### 6. **Shared Kernel**
- OroKernel.Shared for reusable DDD components
- `AuditableDbContext` for audit trails
- Common interfaces and base classes

### 7. **Clean Architecture**
- Core: Domain models and business rules
- Application: Use cases and CQRS
- Infrastructure: EF Core and external services
- API: Minimal endpoints and hosting

These updates ensure type safety, maintainability, and scalability while following DDD best practices.

## Technologies Used
- **ASP.NET Core**: Main framework for the API.
- **OpenIddict**: Implementation of OpenID Connect and OAuth2.
- **Entity Framework Core**: ORM for data access with PostgreSQL.
- **CQRS**: Design pattern for separating commands and queries.
- **MediatR**: In-process messaging library.
- **Aspire**: Distributed application orchestration.
- **PostgreSQL**: Relational database.
- **Serilog**: Logging framework.
- **Docker/Podman**: Containerization.

## Installation and Configuration

### Quick Start with Aspire
1. Clone the repository:
   ```bash
   git clone https://github.com/ojrojas/OroIdentityServer.git
   cd OroIdentityServer
   ```

2. Run with Aspire:
   ```bash
   dotnet run --project src/AppHost/AppHost.csproj
   ```

3. Open Aspire dashboard at http://localhost:15888

### Manual Setup
1. Start PostgreSQL:
   ```bash
   podman run -d --name postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:15
   ```

2. Configure connection string in `appsettings.json`

3. Run migrations:
   ```bash
   dotnet ef database update --project src/Services/OroIdentityServer/OroIdentityServer.Infraestructure
   ```

4. Run the API:
   ```bash
   dotnet run --project src/Services/OroIdentityServer/OroIdentityServer.Api
   ```

## License

This project is licensed under the GNU AGPL v3.0 License. See the [LICENSE](./LICENSE) file for details.

---

Thank you for using OroIdentityServer! If you have any questions or suggestions, feel free to open an issue in the repository.

find . -name "*.csproj" -print0 | xargs -0 dotnet sln add