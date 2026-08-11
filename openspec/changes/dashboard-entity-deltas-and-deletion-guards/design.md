## Context

See proposal.md for motivation. Current state shaping this design:

- The app is a .NET 10 CQRS identity server. The admin console (Blazor `InteractiveAuto`) reads/writes through `IAdminXxxService` (HTTP client in WASM, in-process `ServerAdminXxxService` in server/prerender) that dispatch `IQueryHandler`/`ICommandHandler` via `BuildingBlocks.CQRS`. Handlers return `Result`/`Result<T>` with `Error(Code, Message, ErrorType)`; `HttpResponseMessageFactory.FromResult` maps `ErrorType.Conflict` → 409.
- `Dashboard.razor` currently derives `createdTodayCount` by merging users/roles/identification types and filtering on `CreatedAtUtc.Date`, then feeds that single count into the Users and Tenants StatCards (the "hardcoded" delta the report describes). Sessions/applications/scopes counts come from separate service calls.
- A `ICurrentTenantContext` (scoped) already scopes the users and session-history queries by tenant (via `X-Tenant-Id` header / `oro_tenant` cookie). Roles, tenants, applications, scopes and identification types are global in the data model.
- `DeleteRoleCommandHandler` soft-deletes a role (`role.Deactivate()` + `UpdateAsync`) without checking associations. `Role` has a `RolePermissions` collection; `UserRole` is a join table (`UserId`/`RoleId`).

## Goals / Non-Goals

**Goals:**
- Replace the hardcoded dashboard delta with a single server-side aggregation that returns per-entity "created today" counts (users, roles, tenants, connected users, applications, scopes, identification types) plus the merged "recently created" list, tenant-scoped where the entity is tenant-scoped.
- Ensure every admin create/update/delete/soft-delete form shows success/failure toasts (including the new 409 conflict path).
- Enforce role deletion guards server-side: reject soft-delete when `UserRole` or `RolePermission` records exist for the role.

**Non-Goals:**
- Hard multi-tenant isolation enforcement beyond what the existing `ICurrentTenantContext` scoping already provides for users/sessions.
- Hard-delete of roles (soft-delete via `Deactivate` stays the mechanism).
- Retroactive backfill or reporting of historical "created today" data beyond the current-day aggregation.

## Decisions

### D1: Single dashboard aggregation query instead of per-entity round-trips
Introduce `GetDashboardStatsQuery(Guid? TenantId)` returning a `DashboardStatsDto` with `CreatedToday` counts per entity type and a merged `RecentlyCreated` list (name, type label, href, createdAt). Dispatch it from a new `ServerAdminDashboardService` (and HTTP `AdminDashboardService`), so `Dashboard.razor` makes one call instead of several, and all deltas come from one place.
- Rationale: centralizes the "created today" logic, avoids N round-trips, and directly fixes the shared/hardcoded count.
- Alternative considered: computing deltas in `Dashboard.razor` from the existing list endpoints — rejected because it keeps the logic in the UI and re-introduces the copy/paste drift that caused the bug.

### D2: Count via repository `Any`/count specifications, tenant-scoped where applicable
Add count/spec capabilities to repositories:
- Users: `CountCreatedToday(tenantId)` — `User.CreatedAtUtc.Date == today` and, when `tenantId` set, `User.TenantId == tenantId`.
- Roles/Tenants/Identification types: `CountCreatedToday()` by `CreatedAtUtc.Date == today` (global).
- Connected users: distinct active `UserSession.UserId` (expiresAt > now), filtered to the tenant's users when tenant-scoped.
- Applications/Scopes: count OpenIddict Application/Scope records created today. Verify whether these models expose a creation date (`CreatedAtUtc`/OpenIddict `CreationDate`); if neither exists, exclude them from the "created today" delta and record it as an open question (the stat card then shows total count only).
- Rationale: repository-level specs keep EF translation consistent with the existing `SpecificationEvaluator` and let `Repository<T>` stay generic.
- Alternative considered: `_context.Set<T>().CountAsync(...)` directly in a handler — rejected to keep the infra boundary consistent with the codebase's repository/spec pattern.

### D3: Recently created list built server-side from the same aggregation
The merged list (users, roles, tenants, identification types, applications, scopes) is assembled in the query handler, sorted by `CreatedAtUtc` descending, capped at 5, falling back to most-recent when today is empty. `Dashboard.razor` only renders it.
- Rationale: the "entity name/type" the user asked for comes naturally from the DTO; the UI stops maintaining entity catalogs.

### D4: Role deletion guards in the command, checked before soft-delete
`DeleteRoleCommandHandler` performs two association checks before calling `Deactivate()`:
- `IUserRolesRepository` gains `HasAnyForRoleAsync(RoleId)` (exists on `UserRoles`).
- A role-permission existence check (either `Role.RolePermissions.Any()` via the loaded role, or a repository `Exists` spec on the join table).
If either is true → return `Result.Failure(Error.Conflict(...))` → 409. Otherwise soft-delete and return success.
- Rationale: matches the existing `Result`/`ErrorType.Conflict` → 409 mapping, enforced server-side so the UI and direct API callers behave identically (spec requirement).
- Alternative considered: a DB check constraint / trigger — rejected as inconsistent with the app's domain-rule style and harder to message.

### D5: Toast standardization via a per-page audit, not a new framework
Every admin page already uses `IToastService` (`ShowSuccess`/`ShowError`). The work is an audit: each create/update/delete/soft-delete handler on every entity page (users, roles, tenants, identification types, applications, scopes, sessions) must call `ShowSuccess(...)` on success and `ShowError(L["...FailedToast", (int)response.StatusCode])` on failure. Missing/divergent toasts are added/normalized, and new resource keys are added to all 8 language files.
- Rationale: minimal change, no new dependency; the gap is coverage and consistency, not infrastructure.
- Alternative considered: a shared generic submit helper — rejected as too invasive across varied forms for the current value.

## Risks / Trade-offs

- **OpenIddict Application/Scope creation date may not exist** → Mitigation: confirm the model during implementation; if absent, exclude those from "created today" and keep their total count, and record the open question below. This is a spec-visible behavior only if those entities lack a creation date; verify before finalizing.
- **Check-then-delete race (TOCTOU)** for role guards → Mitigation: acceptable for an admin console; a role acquiring associations between check and soft-delete is rare and non-destructive (soft-delete keeps the row). Note in code.
- **Tenant-scoped deltas depend on the tenant context being set** → Mitigation: `GetDashboardStatsQuery` takes `Guid? TenantId` and the service passes `ICurrentTenantContext.CurrentTenantId` (same pattern as `GetUsersQuery`); null means global, matching existing behavior.
- **Large toast audit surface** → Mitigation: the tasks list enumerates every page/operation explicitly so nothing is missed; tests assert the API status paths that drive the toasts.

## Migration Plan

- No schema migration required (no data model changes). Deploy order: repository/spec additions → query/command changes → service + endpoint wiring → `Dashboard.razor`/page toast changes → resource keys.
- Rollback: the change is additive; reverting keeps the old dashboard delta logic and unguarded role deletion.

## Open Questions

- Do the OpenIddict Application/Scope entities expose a creation date usable for "created today"? Resolved during implementation by inspecting the EF model; if absent, those two stat cards show total counts only and the "recently created" list omits them. This does not change the spec for the other entity types.
