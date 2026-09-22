## 1. Core

- [x] 1.1 Create `Core/Modules/Users/Entities/UserPermission.cs` (`UserId`, `PermissionId`, parameterless ctor for EF).
- [x] 1.2 Add `_permissions` + `Permissions` + `AddPermission`/`RemovePermission` to `User`.
- [x] 1.3 Create `Core/Modules/Users/Repositories/IUserPermissionsRepository.cs`.

## 2. Infrastructure

- [x] 2.1 Create `UserPermissionEntityConfiguration` (composite key, conversions, table `UserPermissions`).
- [x] 2.2 Add `DbSet<UserPermission>` and `ApplyConfiguration` in `OroIdentityAppContext`.
- [x] 2.3 Create `UserPermissionsRepository` + `GetUserPermissionsByUserIdSpecification`.
- [x] 2.4 Register `IUserPermissionsRepository` in `InfraestructureExtensions`.
- [x] 2.5 Include `Permissions` in `GetUserByIdSpecification`.
- [x] 2.6 Union role + direct permissions in `PermissionRepository.GetPermissionNamesByUserIdAsync`.
- [x] 2.7 Generate migration `AddUserPermissions` with `dotnet ef`.
- [x] 2.8 Add `Permission.CreatedAtUtc` + migration `AddPermissionCreatedAt` (for the dashboard recent list).

## 3. Application

- [x] 3.1 Create `AssignPermissionsToUserCommand` + handler (replace-set, diff, mirror `AssignRolesToUserCommand`).
- [x] 3.2 Fix `GetRoleByIdQueryHandler` to read the role with permissions **without tracking** (a tracking read collided with the user graph attached by `UpdateUserAsync`).

## 4. IdentityServer API + client services

- [x] 4.1 `PUT /api/users/{id}/permissions` and `GET /api/users/{id}/effective-permissions` in `AdminUserApiEndpoints`.
- [x] 4.2 `AssignPermissionsToUserAsync` + `GetEffectivePermissionsAsync` in `IAdminUserService`, `ServerAdminUserService`, `AdminUserService`.
- [x] 4.3 `UserModel` + `UserPermissionModel` + `AssignPermissionsRequest`; map in `ServerAdminUserService.MapUser`.

## 5. Client UI

- [x] 5.1 `Permissions.razor` (list + delete).
- [x] 5.2 `PermissionCreate.razor`.
- [x] 5.3 `PermissionDetail.razor` (edit, system-protected).
- [x] 5.4 `UserDetail.razor`: direct-permission checklist + effective-permissions panel.
- [x] 5.5 `MainLayout.razor`: `NavPermissions` entry + `PageTitle` mapping; rename `isMasterAdmin` → `isAdmin`.
- [x] 5.6 `Dashboard.razor`: permissions card + `isAdmin` rename.

## 6. Dashboard query

- [x] 6.1 Include permissions in `GetDashboardStatsQueryHandler.BuildRecentlyCreatedAsync` (href `/permissions/{id}`, `TypeKey = StatPermissions`).

## 7. Localization

- [x] 7.1 Add new keys to the 8 `SharedResources.*.resx` files.

## 8. Tests

- [x] 8.1 Unit: resolution unions role + direct permissions; direct permission with deactivated role; duplicate appears once.
- [x] 8.2 Unit: `AssignPermissionsToUserCommandHandler` add/remove/idempotent/not-found/unknown.
- [x] 8.3 Unit: role read after assignment does not conflict (`RoleReadTrackingConflictTests`).
- [x] 8.4 Integration: `PUT /api/users/{id}/permissions` round-trip + effective endpoint + unknown rejected.

## 9. Docs

- [x] 9.1 `docs/authorization.md`: direct user permissions.
- [x] 9.2 `README.md`: menu/page, endpoints, dashboard.

## 10. Verification

- [x] 10.1 `dotnet build OroIdentityServer.slnx` — succeeded.
- [x] 10.2 `dotnet test tests/Infraestructure.UnitTests` — 64 passed.
- [x] 10.3 Integration (`UserPermissionsApiTests`, `RolePermissionsApiTests`, `ApplicationsListTests`, `AuthTokenLoginTests`, `ConsentFlowTests`) — passed.
- [ ] 10.4 Full `tests/Server.Tests` suite — blocked by the local Aspire/Postgres container state (the AppHost now uses a persistent Postgres 18 volume; repeated runs leave persistent containers that make the fixture hang). Re-run after cleaning containers/volumes.
