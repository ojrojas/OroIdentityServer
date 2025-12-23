# OroIdentityServer

OroIdentityServer is an identity and authentication management system based on ASP.NET Core. This project implements an identity server using OpenIddict, supporting authentication, authorization, and management of users, roles, applications, and scopes.

## Key Features

### 1. **Authentication and Authorization**
- Implementation of authentication and authorization flows using OpenIddict.
- Minimal endpoints for authorization (`/connect/authorize`, `/connect/token`, `/connect/logout`).
- Support for OAuth2 and OpenID Connect authorization flows.

### 2. **User Management**
- Full CRUD for users.
- Commands and queries for operations such as creating, updating, deleting, and retrieving users.
- Password verification and user authentication handling.

### 3. **Role Management**
- Full CRUD for roles.
- Endpoints for assigning and managing user roles.

### 4. **Application Management**
- CRUD for applications registered in OpenIddict.
- Endpoints for creating, updating, deleting, and listing applications.

### 5. **Scope Management**
- CRUD for OpenIddict scopes.
- Endpoints for creating, updating, deleting, and listing scopes.

### 6. **Modular Architecture**
- Separation of responsibilities into projects:
  - **OroIdentityServer.Api**: Contains minimal endpoints and API configuration.
  - **OroIdentityServer.Application**: Implements business logic with commands and queries (CQRS).
  - **OroIdentityServer.Core**: Defines shared interfaces and models.
  - **OroIdentityServer.Infraestructure**: Implements repositories and data access.

## Project Structure

```
OroIdentityServer/
├── src/
│   ├── Services/
│   │   ├── OroIdentityServer.Api/          # Endpoints and API configuration
│   │   ├── OroIdentityServer.Application/  # Business logic (CQRS)
│   │   ├── OroIdentityServer.Core/         # Shared interfaces and models
│   │   ├── OroIdentityServer.Infraestructure/ # Repositories and data access
│   └── ...
├── README.md
├── LICENSE
└── OroIdentityServer.sln
```

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

The project has been updated to follow Domain-Driven Design (DDD) principles. Below are the key changes:

### 1. **Aggregate Roots**
- Introduced `AggregateRoot<T>` base class to manage domain events.
- Added `RaiseDomainEvent` method to handle domain events within aggregates.

### 2. **Value Objects**
- Implemented value objects like `RoleClaimId`, `RoleId`, and `UserId` to encapsulate identity logic.
- Added `TryParse` methods to enable seamless integration with ASP.NET endpoints.

### 3. **Repositories**
- Updated repository interfaces (e.g., `IRolesRepository`) to align with aggregate boundaries.
- Implemented methods for managing role claims and user roles.

### 4. **CQRS Enhancements**
- Refined command and query handlers to work with value objects and aggregates.
- Ensured separation of concerns between read and write operations.

### 5. **Shared Kernel**
- Centralized shared logic in the `Shared` project.
- Added base classes like `BaseEntity`, `BaseValueObject`, and interfaces like `IAggregateRoot`.

### 6. **Null Safety**
- Addressed null reference issues across the codebase.
- Added null checks and improved error handling in repositories and services.

### 7. **Type Safety**
- Replaced `Guid` with strongly-typed identifiers (e.g., `RoleClaimId`, `RoleId`) for better type safety.
- Updated endpoints and handlers to use these types.

These changes improve the maintainability, scalability, and robustness of the system while adhering to DDD principles.

## Technologies Used
- **ASP.NET Core**: Main framework for the API.
- **OpenIddict**: Implementation of OpenID Connect and OAuth2.
- **Entity Framework Core**: ORM for data access.
- **CQRS**: Design pattern for separating commands and queries.

## Installation and Configuration

1. Clone the repository:
   ```bash
   git clone https://github.com/ojrojas/OroIdentityServer.git
   ```

2. Navigate to the project directory:
   ```bash
   cd OroIdentityServer
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Configure the database in `appsettings.json`.

5. Apply migrations:
   ```bash
   dotnet ef database update
   ```

6. Run the project:
   ```bash
   dotnet run --project src/Services/OroIdentityServer/OroIdentityServer.Api
   ```

## License

This project is licensed under the GNU AGPL v3.0 License. See the [LICENSE](./LICENSE) file for details.

---

Thank you for using OroIdentityServer! If you have any questions or suggestions, feel free to open an issue in the repository.

find . -name "*.csproj" -print0 | xargs -0 dotnet sln add