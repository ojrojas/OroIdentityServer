## 1. Repository / data access layer

- [x] 1.1 Inspect the OpenIddict Application/Scope EF models and confirm whether they expose a creation date (`CreatedAtUtc` or OpenIddict `CreationDate`); record the finding.
- [x] 1.2 Add `CountCreatedTodayAsync(DateTime today, Guid? tenantId, CancellationToken ct)` to `IUserRepository`/`UserRepository` (filter `CreatedAtUtc.Date == today`, and `TenantId == tenantId` when provided).
- [x] 1.3 Add `CountCreatedTodayAsync(DateTime today, CancellationToken ct)` to `IRoleRepository`/`RoleRepository`, `ITenantRepository`/`TenantRepository`, and `IIdentificationTypeRepository`/impl.
- [x] 1.4 Add active connected-users count capability (`IUserSessionRepository`): distinct `UserId` where `ExpiresAt > UtcNow`, tenant-scoped via user lookup when a tenant is provided.
- [x] 1.5 Add application/scope created-today counts (if a creation date exists per 1.1; otherwise implement total-count only and note it).
- [x] 1.6 Add `HasAnyForRoleAsync(RoleId roleId, CancellationToken ct)` to `IUserRolesRepository`/`UserRolesRepository` (exists on `UserRoles`).
- [x] 1.7 Add role-permission existence check (spec/count on `RolePermission` for a role, or via `Role.RolePermissions`).

## 2. Dashboard aggregation

- [x] 2.1 Create `DashboardStatsDto` and `RecentEntityDto` (name, type label key, href, avatar seed, createdAt).
- [x] 2.2 Create `GetDashboardStatsQuery(Guid? TenantId)` + `GetDashboardStatsQueryHandler` that aggregates created-today counts per entity type and the merged "recently created" list (users, roles, tenants, identification types, and applications/scopes where available), sorted by createdAt descending, capped at 5, falling back to most-recent when today is empty.
- [x] 2.3 Wire `GetDashboardStatsQuery` into a new `ServerAdminDashboardService` (in-process) and HTTP `AdminDashboardService` + `IAdminDashboardService` interface; pass `ICurrentTenantContext.CurrentTenantId`.
- [x] 2.4 Add `GET /api/dashboard/stats` endpoint (ManagerOrAdmin).
- [x] 2.5 Update `Dashboard.razor` to consume the single stats response: each StatCard shows its own delta and name, connected users from the response, and the recently-created row renders the server-side list (removing the hardcoded `createdTodayCount`).

## 3. Role deletion guards

- [x] 3.1 Update `DeleteRoleCommandHandler`: check `HasAnyForRoleAsync` (UserRole) and the role-permission existence before `role.Deactivate()`; return `Result.Failure(Error.Conflict(...))` with a clear message when either association exists.
- [x] 3.2 Update `Roles.razor`/`RoleDetail.razor` delete flow to surface the conflict failure toast (spec: failure toast with status).
- [x] 3.3 Add resource keys for the guard messages (en + all language files).

## 4. CRUD toast standardization

- [x] 4.1 Audit and fix toast coverage for Users (create/update/delete/lock/unlock).
- [x] 4.2 Audit and fix toast coverage for Roles (create/update/delete/soft-delete incl. conflict path).
- [x] 4.3 Audit and fix toast coverage for Tenants (create/update/activate/suspend/delete/add-user).
- [x] 4.4 Audit and fix toast coverage for Identification Types (create/update/delete).
- [x] 4.5 Audit and fix toast coverage for Applications (create/update/delete).
- [x] 4.6 Audit and fix toast coverage for Scopes (create/update/delete).
- [x] 4.7 Audit and fix toast coverage for Sessions/user-sessions (disconnect/terminate).
- [x] 4.8 Ensure every write handler calls `ShowSuccess` on success and `ShowError(L["...FailedToast", (int)status])` on failure; add missing resource keys to all 8 language files.

## 5. Tests

- [x] 5.1 Unit tests for created-today counts (users tenant-scoped and global; roles/tenants/identification types).
- [x] 5.2 Unit tests for role deletion guards (reject with UserRole; reject with RolePermission; allow when no associations).
- [x] 5.3 Integration tests: `GET /api/dashboard/stats` returns correct per-entity deltas; `DELETE /api/roles/{id}` returns 409 when the role has users or permissions and 204/200 when clean.
- [x] 5.4 Integration test: delete-role conflict surfaces a toast-facing 409 status in the API response.

## 6. Verification

- [x] 6.1 Build the solution (no warnings/errors) and run the full test suite.
- [x] 6.2 Manually verify dashboard deltas and recently-created list reflect real data for each entity type.
