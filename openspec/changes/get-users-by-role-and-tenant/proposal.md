## Why

The admin user catalog currently lists all users in a tenant (`GET /api/users`) but provides no way to filter users by their assigned role. Administrators managing large user bases need to quickly see which users hold a specific role (e.g., all Managers, all Administrators) within a tenant. Without this, role auditing and user management require manual cross-referencing.

## What Changes

- Add a new query `GetUsersByRoleAndTenantQuery` that returns users associated with a specific catalogue role, with tenant scoping controlled by the caller's role.
- Expose a new endpoint `GET /api/users/by-role/{roleId}?tenantId=<guid>` that dispatches the query.
- Enforce role-based access at the service layer:
  - **Administrator** callers may query any role (including Administrator) across all tenants. `tenantId` is optional — when omitted, results span all tenants.
  - **Non-Administrator** callers may only query non-Administrator roles. They MUST provide `tenantId` and are restricted to their own home tenant.

## Capabilities

### New Capabilities

- `users/get-users-by-role-and-tenant`: The system SHALL return users that belong to a given catalogue role, with tenant scoping controlled by the caller's role. Administrator may query any role across all tenants; non-Administrator may only query non-Administrator roles within their own tenant.

### Modified Capabilities

- None.

## Impact

- **Application**: New query record, handler, and response in `Modules/Users/Queries`.
- **Core**: New repository method `GetUsersByRoleIdAsync` on `IUserRepository`.
- **Infrastructure**: Repository implementation with role + tenant filtering.
- **IdentityServer**: New Minimal API route in `AdminUserApiEndpoints`, new method on `IAdminUserService` / `ServerAdminUserService`.
- **IdentityServer.Client**: New client method on `IAdminUserService` / `AdminUserService` and corresponding HTTP call.
