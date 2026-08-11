## Why

The admin dashboard shows a "created today" count that is hardcoded/incorrect for most entities (it only reflects users, and even then inaccurately), so operators cannot trust the numbers at a glance. In addition, CRUD feedback (toasts) is inconsistent across entity forms, and roles can be deleted even when they are still assigned to users or hold role permissions, which corrupts references and breaks authorization.

## What Changes

- Replace the hardcoded "created today" dashboard delta with real, per-entity counts computed from the database (users, roles, tenants, connected users, applications, scopes, identification types). Each stat card shows the entity's own name and its own count created during the current calendar day.
- Make the "recently created" dashboard row fully dynamic across all supported entity types instead of only users/roles/identification types.
- Audit and standardize toast feedback on **all** admin CRUD forms for **all** entities: create, update, delete, and soft-delete operations must show a success toast on success and a failure toast (with HTTP status) on failure. Missing toasts are added; inconsistent ones are normalized.
- Add deletion guards for roles: a role cannot be soft-deleted while it still has any assigned `UserRole` records or any associated `RolePermission` records. The API returns a `Conflict` result with a clear message, and the UI shows the failure toast.
- Apply the same guard principle to any other entity whose deletion would leave dangling references (verified per entity during design).

## Capabilities

### New Capabilities

- `dashboard/entity-deltas`: The admin dashboard must render live per-entity "created today" counts (with the entity's name) for users, roles, tenants, connected users, applications, scopes and identification types, based on real data rather than a hardcoded value.
- `crud/operation-feedback`: Every admin CRUD operation (create, update, delete, soft-delete) across all managed entity types must surface a success or failure toast to the user.
- `roles/deletion-guards`: A role must not be deletable while it has associated users (`UserRole`) or associated role permissions (`RolePermission`); deletion attempts must be rejected with a `Conflict` result and a clear message.

### Modified Capabilities

- None. No existing specs exist yet; all behavior is captured in the new capabilities above.

## Impact

- **Application (CQRS)**: `GetUsersQuery`, new/updated dashboard aggregation queries (per-entity today counts, recently created), `DeleteRoleCommandHandler`, and any deletion guards for other entities.
- **Infrastructure**: repository queries/specifications for counting entities created today and for checking role associations (`UserRole`, `RolePermission`).
- **IdentityServer.Client UI**: `Dashboard.razor` (per-entity deltas + recent list), all entity create/detail/list pages (toast standardization), role deletion flow (conflict feedback).
- **API**: `POST /api/roles/{id}` (soft-delete) behavior changes to reject deletion while associations exist; HTTP 409 conflict responses.
- **Tests**: unit tests for deletion guards and today-count aggregation; integration tests for dashboard deltas and toast-facing API status codes.
