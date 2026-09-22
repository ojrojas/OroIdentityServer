## Why

The domain permission catalogue and the role↔permission assignment shipped, but the capability is not reachable from the admin console and cannot be granted directly to a user:

- There is no **menu entry** and no **page** for the permission catalogue: `GET/POST/PUT/DELETE /api/permissions` and `IAdminPermissionService` exist, but administrators can only manage permissions through the raw API.
- The **dashboard** has no permissions widget, so operators cannot see the catalogue at a glance.
- Users can only obtain permissions through their roles (`UserRole → Role → RolePermission → Permission`). There is no way to grant a permission to a single user without inventing a role for it.

## What Changes

- **Menu**: add a `Permissions` navigation entry (Admin/Administrator) and a page-title mapping.
- **Catalogue UI**: add list/create/detail pages for the permission catalogue using the existing `IAdminPermissionService`; system permissions (`IsSystem`) cannot be edited or deleted.
- **Dashboard**: add a permissions stat card (total count) and include permissions in the "recently created" list.
- **Direct user permissions**: add a `UserPermission` entity so a user's effective permissions become `union(role permissions, direct user permissions)`. The single resolution point (`PermissionRepository.GetPermissionNamesByUserIdAsync`) is updated, so the cookie, access token, id token and `userinfo` automatically carry direct permissions too.
- **Assignment API/UI**: `PUT /api/users/{id}/permissions`, `IAdminUserService.AssignPermissionsToUserAsync`, and a direct-permission checklist plus a read-only effective-permissions panel in `UserDetail.razor`.
- **Docs**: extend `docs/authorization.md` and the README.

## Capabilities

### New Capabilities

- `admin/permissions-catalog-ui`: the admin console SHALL list, create, edit and delete domain permissions, protecting system permissions from modification.
- `dashboard/permissions-stats`: the dashboard SHALL show the permission count and recent permissions.
- `authorization/direct-user-permissions`: the IdP SHALL support permissions granted directly to a user, combined with role-derived permissions when resolving a user's effective permissions.

### Modified Capabilities

- `authorization/permission-claims`: permission resolution now also includes directly-granted user permissions (the claim/destination contract is unchanged).

## Impact

- **Core**: new `UserPermission` entity, `User` aggregate collection, new `IUserPermissionsRepository`.
- **Infrastructure**: EF configuration + `DbSet`, repository + specifications, `GetUserByIdSpecification` include, union resolution in `PermissionRepository`, DI registration, migration `AddUserPermissions`.
- **Application**: `AssignPermissionsToUserCommand` + handler.
- **IdentityServer**: `PUT /api/users/{id}/permissions` endpoint; `ServerAdminUserService`; `AdminUserService`; `UserModel`.
- **Client UI**: `Permissions.razor`, `PermissionCreate.razor`, `PermissionDetail.razor`, `UserDetail.razor`, `MainLayout.razor`, `Dashboard.razor`.
- **Localization**: new keys in the 8 resource files.
- **Docs**: `docs/authorization.md`, `README.md`.
- **No breaking changes**; additive schema only.
