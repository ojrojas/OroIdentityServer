## Context

See proposal.md. Current state shaping this design:

- Two session models exist but are not wired into the auth flow: `UserSession` aggregate (`UserSession.cs:15`, fields: device, sessionToken, expiresAt, lastActivity, ipAddress, userAgent, location) and `Session` entity (`Session.cs:25`, OpenIddict `AuthorizationId`, tenant, ip, country, started/ended). Neither is created at login or token issuance.
- Admin endpoints exist for `UserSession` (`AdminUserSessionsEndpoints.cs`: by-user, active, active-count, create, deactivate, terminate-all) and a read-only by-user endpoint for `Session` (`AdminSessionsEndpoints.cs`).
- `DeactivateUserSessionCommandHandler` only sets `ExpiresAt = now` (`UserSession.cs:39`) — it does NOT revoke OpenIddict authorizations/tokens, so the UI "Disconnect" button (`Sessions.razor:195`) does not actually log the user out.
- `TerminateSessionCommandHandler` (Sessions module) does revoke authorizations/tokens via reflection over `IOpenIddictAuthorizationManager`/`IOpenIddictTokenManager` but is not exposed by any endpoint/service.
- Token issuance happens in `AuthorizationController.Exchange` (`AuthorizationController.cs:391`); the restored principal carries the subject and the authorization id claim (`Claims.AuthorizationId`). Admin login is `AuthEndpoints` `/auth/login` (`AuthEndpoints.cs:25`).
- The typed non-generic managers (`OpenIddict.Abstractions 8.0.0-preview.2`) expose: `IOpenIddictAuthorizationManager.FindByIdAsync/FindBySubjectAsync/TryRevokeAsync/RevokeBySubjectAsync`, and `IOpenIddictTokenManager.FindByAuthorizationIdAsync/RevokeByAuthorizationIdAsync/RevokeBySubjectAsync/TryRevokeAsync`.

## Goals / Non-Goals

**Goals:**
- One session record per active login (OIDC token issuance and admin cookie login), with connection origin and OpenIddict linkage.
- Admin visibility of active sessions (per user and global) with origin, and one/two-click termination that genuinely revokes access (remote logout) with an optional force-logout/lock.
- Remove the reflection-based revocation and the duplicate `Session` model.

**Non-Goals:**
- IP geolocation/country resolution (captured only as origin metadata; no external geo service).
- Live-push logout to connected browsers (termination makes the next refresh/API call fail; the client's 401 handling redirects to login).
- Per-request DB validation of every access token (tokens are rejected by revocation via OpenIddict validation).

## Decisions

### D1: Consolidate into the `UserSession` aggregate
Extend `UserSession` with `TenantId`, `ClientId` (nullable), `AuthorizationId` (nullable), `Country` (nullable), `EndedAtUtc` (nullable) and keep device/sessionToken/expiresAt/lastActivity/ipAddress/userAgent/location. Active ⇔ `EndedAtUtc == null && ExpiresAt > UtcNow`. Delete the `Session` entity, `ISessionRepository`/`SessionRepository`, and the `Sessions` Application module (its `CreateSessionCommand`/`GetUserSessionsQuery`/`TerminateSessionCommand` are folded into the consolidated commands/queries). EF migration merges `Sessions` rows into the consolidated `UserSessions` table (mapping `SessionId→UserSessionId`, copying ip/country/started/ended) and drops the `Sessions` table.
- Rationale: one aggregate/table for "a login" removes the dual-model confusion; the OpenIddict `AuthorizationId` is what enables real revocation.
- Alternative considered: keeping both and just wiring them — rejected: it doubles the surface and the user explicitly lacks a coherent audit view.

### D2: Record sessions at token issuance and admin login
In `AuthorizationController.Exchange` (both `IsAuthorizationCodeGrantType` and `IsRefreshTokenGrantType` branches, after the successful `SignIn` decision) dispatch a `CreateUserSessionCommand` with: userId from `Claims.Subject`, `AuthorizationId` from `result.Principal.GetClaim(Claims.AuthorizationId)`, `ClientId` from `request.ClientId`, tenant from `ICurrentTenantContext` (fallback: user's default tenant when null), origin from the helper below, and `ExpiresAt` derived from the issued access token expiry when available (else configured access-token lifetime). In `AuthEndpoints` `/auth/login`, after the successful cookie sign-in, dispatch the same command with `ClientId="admin"`, no `AuthorizationId`, same origin capture.
- Rationale: these are the two real "a session began" points; refresh-token grants update the existing session's `LastActivityAt` (via `GetByTokenAsync`/by `AuthorizationId`) instead of creating duplicates.
- Alternative considered: an OpenIddict server event handler (`IOpenIddictServerEventHandler` on `TokenValidated`) — cleaner hook but the codebase already centralizes exchange logic in `AuthorizationController`; wiring there keeps the change localized.

### D3: Origin capture helper
Add a small helper (e.g. `ConnectionOriginCapture.Capture(HttpContext)`) returning `{ IpAddress, UserAgent, Device }`:
- IP: first hop of `X-Forwarded-For` when present, else `Connection.RemoteIpAddress?.ToString()` (empty string when null).
- User-Agent: raw `User-Agent` header (empty string when missing).
- Device: a lightweight UA parser producing e.g. `Chrome 126 · Windows 11`, falling back to `Unknown` (no new dependency).
Rationale: satisfies the spec's proxy/direct/missing-UA scenarios without adding a UA library.

### D4: Termination revokes OpenIddict authorizations/tokens with typed managers
Rework `DeactivateUserSessionCommandHandler` and `TerminateAllUserSessionsCommandHandler` (consolidated module) to, in addition to marking the session(s) ended:
- Resolve the linked authorization via `authorizationManager.FindByIdAsync(authorizationId)` and `TryRevokeAsync` it.
- Revoke all its tokens via `tokenManager.RevokeByAuthorizationIdAsync(authorizationId)`.
- `DeactivateUserSessionCommand` gains an optional `ForceLogout` flag: when set, additionally revoke every session of the user (`authorizationManager.RevokeBySubjectAsync` + `tokenManager.RevokeBySubjectAsync`) and, when a `LockUser` flag is also set, lock the user account (reuse the existing `SecurityUser` lockout / `LockUserCommand` path).
- Delete the reflection-based `TerminateSessionCommandHandler` (Sessions module) — the typed managers replace it.
- Rationale: the typed managers give the exact same effect with compile-time safety; the reflection monster is the source of the "does not really work" gap.
- Alternative considered: keeping reflection for compatibility — rejected, it is unmaintainable and now unnecessary.

### D5: Admin API + client service + UI
Consolidate the admin surface under `/api/admin/user-sessions/*`:
- `GET /active` (global, origin fields), `GET /by-user/{userId}`, `GET /active-count` (kept).
- `POST /{id}/terminate` (body: `forceLogout`, `lock`) → revoke + optionally full logout/lock.
- `POST /terminate-all/{userId}` (body: `forceLogout`, `lock`).
- Update `IAdminUserSessionService`/`ServerAdminUserSessionService`/client `AdminUserSessionService` and `UserSessionModel` with origin fields (ip, userAgent, device, clientId, startedAt, lastActivity, expires, endedAt) and the terminate calls.
- `Sessions.razor` merges the active list and history into one table and the Disconnect action calls `POST {id}/terminate`; a "force logout + lock" action is offered as an explicit secondary option.
- Drop the now-redundant `AdminSessionsEndpoints` (`/api/admin/sessions/*`) and `IAdminSessionService`/`AdminSessionService` client pair (replaced by the consolidated service).
- Rationale: the existing `Sessions.razor` already lays out the UX; the change fills in the origin columns and makes Disconnect actually revoke.
- Alternative considered: separate pages for active vs history — rejected, the user wants one audit view.

### D6: Tenant scoping of listings
`GetAllActiveUserSessionsQuery`/`GetUserSessionsByUserQuery` take the current tenant via `ICurrentTenantContext` and filter `UserSession.TenantId` when set (matching the `GetUserSessionsQuery` pattern already used for `Session`); null = global (admin).
- Rationale: consistency with the existing tenant-scoping convention in `ServerAdminSessionService.GetByUserAsync`.

## Risks / Trade-offs

- **Dual grant types may double-record** → refresh grants update the existing session by `AuthorizationId` instead of inserting; mitigates duplicates.
- **Access token already issued keeps working until it hits the API** → OpenIddict revocation invalidates it on next validation; access-token lifetimes are short, so the "kick to login" is prompt. Hard per-request DB checks are explicitly a non-goal.
- **Force-logout locks the account** → Locking is opt-in per action (spec scenario), never the default, so admins don't lock users accidentally.
- **Migration of `Sessions` rows** → Straight column copy; `AuthorizationId` preserves revocation linkage for migrated OIDC sessions. Rollback keeps old tables until verified.
- **Admin cookie sessions have no authorization id** → termination for them is local (mark ended) since there are no OIDC tokens to revoke; cookie expiry is short.

## Migration Plan

1. Extend `UserSession` aggregate + repository + EF configuration; add migration (new columns + merge `Sessions` → `UserSessions`, drop `Sessions`).
2. Rework `CreateUserSessionCommand`/queries/termination commands (typed OpenIddict managers, force-logout/lock).
3. Wire recording into `AuthorizationController.Exchange` and `AuthEndpoints` login; add origin-capture helper.
4. Update admin endpoints, client services/models, and `Sessions.razor`.
5. Remove `Session` entity, `Sessions` module, `AdminSessionsEndpoints`, and the client `IAdminSessionService` pair.
6. Tests: unit (session lifecycle, force-logout semantics), integration (token-issuance recording, origin capture via `X-Forwarded-For`, revocation → invalid grant on refresh, terminate-all).
- Rollback: feature is additive at runtime except the `Sessions` table merge; deploy migration last after code, keep a pre-merge backup.

## Open Questions

- None that change the specs or task breakdown. Minor unknowns (exact access-token expiry claim availability, user-default-tenant lookup) are resolved during implementation without altering the agreed behavior.
