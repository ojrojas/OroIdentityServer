## 1. Domain / Repository interface

- [x] 1.1 Add `GetUsersByRoleIdAsync(Guid roleId, Guid? tenantId, CancellationToken)` to `IUserRepository` (`Core/Modules/Users/Repositories/IUserRepository.cs`).

## 2. Infrastructure / Repository implementation

- [x] 2.1 Implement `GetUsersByRoleIdAsync` in `UserRepository` (`Infraestructure/Repositories/UserRepository/UserRepository.cs`): join `User.UserRoles` → `Role`, filter by `RoleId` and optional `TenantId`, include eager-loaded roles (same as `GetAllUsersSpecification` pattern).
- [x] 2.2 Add a specification `GetUsersByRoleSpecification` in `Infraestructure/Specifications/` if the repository uses the specification pattern for this query.

## 3. Application layer (CQRS)

- [x] 3.1 Create `GetUsersByRoleAndTenantQuery` record in `Application/Modules/Users/Queries/GetUsersByRoleAndTenantQuery.cs` with parameters `Guid RoleId` and `Guid? TenantId`, implementing `IQuery<GetUsersByRoleAndTenantQueryResponse>`.
- [x] 3.2 Create `GetUsersByRoleAndTenantQueryResponse` record in `Application/Modules/Users/Queries/GetUsersByRoleAndTenantQueryResponse.cs` extending `BaseResponse<IEnumerable<User>>`.
- [x] 3.3 Create `GetUsersByRoleAndTenantQueryHandler` in `Application/Modules/Users/Queries/GetUsersByRoleAndTenantQueryHandler.cs`: call `repository.GetUsersByRoleIdAsync`, return empty list (not error) when no users match.

## 4. Service layer (authorization + dispatch)

- [x] 4.1 Add `GetUsersByRoleAndTenantAsync(Guid roleId, Guid? tenantId, CancellationToken)` to `IAdminUserService` (`IdentityServer.Client/interfaces/IAdminUserService.cs`).
- [x] 4.2 Implement in `ServerAdminUserService` (`IdentityServer/Services/ServerAdminUserService.cs`):
  - Resolve the role via `GetRoleByIdQuery` to get its name.
  - If role name is `Administrator` and caller lacks the `Administrator` catalogue role claim → return `403 Forbidden`.
  - Apply tenant scoping via `IsAccessibleTenantAsync`.
  - Dispatch `GetUsersByRoleAndTenantQuery` and map results to `UserModel` list.
- [x] 4.3 Implement in client `AdminUserService` (`IdentityServer.Client/Services/AdminUserService.cs`): HTTP GET to `/api/users/by-role/{roleId}?tenantId={tenantId}`.

## 5. API endpoint

- [x] 5.1 Add `GET /{roleId:guid}/by-role` route to the `/users` group in `AdminUserApiEndpoints.cs`, accepting `roleId` path param and optional `tenantId` query param, delegating to `IAdminUserService.GetUsersByRoleAndTenantAsync`.

## 6. Verification

- [x] 6.1 Unit test: Administrator can query Administrator role users.
- [x] 6.2 Unit test: Non-Administrator calling with Administrator role returns 403.
- [x] 6.3 Unit test: Non-Administrator can query Manager role users.
- [x] 6.4 Unit test: Tenant admin cannot query users from a different tenant.
- [x] 6.5 Integration test: endpoint returns correct filtered users with proper tenant scoping.
- [x] 6.6 `dotnet build` the solution and run affected test projects.
