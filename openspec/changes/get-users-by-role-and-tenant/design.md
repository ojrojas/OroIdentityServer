## Context

The admin user catalog exposes `GET /api/users` which returns all users in the current tenant. There is no mechanism to filter by role. The `UserRole` join entity links `User` to `Role` (many-to-many), and the `Role` aggregate holds the catalogue role name (`Administrator`, `Admin`, `Manager`, `User`). The existing `GetUsersQuery` filters by tenant only.

Authorization is layered:
- Middleware policies (`ManagerOrAdmin`, `AdminOnly`) gate endpoint groups.
- Service-level checks in `ServerAdminUserService` enforce master-admin vs tenant-admin scoping for write operations.
- The new query needs a role-based access model:
  - **Administrator** callers may query any role (including Administrator) across all tenants. `tenantId` is optional — when omitted, results span all tenants.
  - **Non-Administrator** callers may only query non-Administrator roles. They MUST provide `tenantId` and are restricted to their own home tenant.

## Goals / Non-Goals

**Goals:**
- Provide a query that returns users filtered by catalogue role and tenant.
- Enforce the Administrator-role visibility rule at the service layer.
- Reuse existing infrastructure (repository, CQRS, Minimal API) patterns.

**Non-Goals:**
- Filtering by multiple roles simultaneously (single role per request).
- Pagination (the existing `GetUsersQuery` does not paginate; this query follows the same convention).
- Changing the existing `GET /api/users` endpoint.

## Decisions

### D1: New CQRS query alongside existing `GetUsersQuery`

Add `GetUsersByRoleAndTenantQuery(Guid RoleId, Guid? TenantId)` in `Application/Modules/Users/Queries/`. The handler loads users via a new repository method `GetUsersByRoleIdAsync(Guid roleId, Guid? tenantId)` and returns `IEnumerable<User>`. This keeps the existing `GetUsersQuery` untouched and follows the established one-query-one-handler pattern.
- Rationale: additive, no risk to existing behavior.
- Alternative: extend `GetUsersQuery` with optional `RoleId` — rejected because it conflates two distinct use cases and complicates the handler with conditional logic.

### D2: Repository method with role + tenant filtering

Add `GetUsersByRoleIdAsync(Guid roleId, Guid? tenantId, CancellationToken)` to `IUserRepository`. The EF implementation joins `UserRole` → `Role` and filters by `RoleId` and optional `TenantId`, returning users with eager-loaded roles (same as `GetAllUsersSpecification`).
- Rationale: filtering at the database level avoids loading all users into memory.
- Alternative: filter in-memory after `GetAllUsersAsync` — rejected for performance with large user bases.

### D3: Service-level role-based access control

In `ServerAdminUserService.GetUsersByRoleAndTenantAsync`:
1. Resolve the requested role via `GetRoleByIdQuery` to determine its name.
2. **If the caller is Administrator:**
   - May query any role (including Administrator).
   - `tenantId` is optional — when null, query spans all tenants.
3. **If the caller is NOT Administrator:**
   - If the requested role is `Administrator` → return `403 Forbidden`.
   - If `tenantId` is not provided → return `400 Bad Request`.
   - If `tenantId` does not match the caller's home tenant → return `403 Forbidden`.
4. Dispatch `GetUsersByRoleAndTenantQuery` and map results.

This check is deliberately at the service layer (not middleware) because it depends on both the caller's role and the runtime value of the requested role.
- Rationale: consistent with how `AssignRolesToUserAsync` does service-level authorization.
- Alternative: a custom authorization handler — rejected as over-engineering for a single check.

### D4: Minimal API route

Add `GET /{roleId:guid}/by-role` to the `/users` route group in `AdminUserApiEndpoints.cs`, accepting `roleId` as a path parameter and `tenantId` as an optional query parameter. The endpoint delegates to `IAdminUserService.GetUsersByRoleAndTenantAsync`.
- Rationale: follows the existing route convention (`/users/{id}`, `/users/{id}/roles`).
- Alternative: separate `/api/user-roles` group — rejected as it fragments the user catalog surface.

### D5: Client service and model

Add `GetUsersByRoleAndTenantAsync(Guid roleId, Guid? tenantId)` to `IAdminUserService` and its implementations (`ServerAdminUserService`, client `AdminUserService`). The response reuses the existing `UserModel` collection — no new DTO is needed.
- Rationale: the response shape is identical to `GetUsersAsync`.

## Risks / Trade-offs

- **Performance with large user bases** → mitigated by database-level filtering via the specification; no full table scan.
- **Role name hardcoded for the security check** → the `Administrator` role name is a catalogue constant (`CatalogueRole`) already used throughout the codebase; adding a constant reference is acceptable.
- **No pagination** → consistent with existing `GetUsersQuery`; pagination can be added later as a separate change if needed.
