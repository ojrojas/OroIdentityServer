## Why

There is no usable session audit. Two parallel session models exist (`UserSession` and `Session`) but neither is recorded during authentication, so the server cannot answer "which sessions are active", "which application a user is logged into", or "from where (IP/device) they connected". Terminating a session/revoking tokens exists only as a reflection-based `TerminateSessionCommand` that is not exposed through any API or UI.

## What Changes

- Consolidate session tracking into a single `UserSession` aggregate that links to OpenIddict: add `TenantId`, `AuthorizationId`, `Country`, `EndedAtUtc`/revocation state, keeping existing origin fields (IP, User-Agent, device, location). The separate `Session` entity and `Sessions` module are replaced by the consolidated one. **BREAKING** for the old `/api/admin/sessions/*` surface, which is superseded by the consolidated `/api/admin/user-sessions/*` API.
- Record a session automatically at **token issuance** (`/connect/token` authorization-code and refresh-token grants) and at **admin login** (`/auth/login`), capturing origin from the request: client IP (honoring `X-Forwarded-For`), raw `User-Agent`, and parsed device/browser/OS.
- Track active state from the OpenIddict authorization lifecycle: a session is active while its authorization is valid and not expired.
- Add admin operations: list active sessions (with origin info), terminate a single session, and terminate all sessions of a user.
- Terminate a session by **revoking the OpenIddict authorization and all its tokens** (access + refresh) using the typed `IOpenIddictAuthorizationManager`/`IOpenIddictTokenManager` API (replacing the reflection-based implementation), so the client application receives a 401 on its next call/refresh and redirects to login (remote logout).
- Make the forced logout/lock **optional per action**: when the admin requires it, the terminate operation also revokes every session of the user (full logout from all apps) and may lock the user account.

## Capabilities

### New Capabilities

- `sessions/audit-and-revocation`: The system SHALL record user sessions on token issuance and admin login with connection origin (IP, user-agent, device); SHALL expose active sessions per user/global with that origin; SHALL support terminating a single or all sessions of a user by revoking the OpenIddict authorization and its tokens; and SHALL support an optional force-logout/lock mode chosen by the admin.

### Modified Capabilities

- None. No existing specs exist for sessions; this is the first.

## Impact

- **Core**: `UserSession` aggregate extended (`TenantId`, `AuthorizationId`, `Country`, revocation state); `Session` entity/module removed or absorbed.
- **Application**: session recording at the auth flow; rework `TerminateSessionCommand` with typed OpenIddict managers; new/updated queries and commands (active list with origin, terminate one/all with optional lock).
- **IdentityServer**: `AuthorizationController.Exchange` and `AuthEndpoints` record sessions with origin; admin endpoints expose active sessions and termination.
- **IdentityServer.Client UI**: admin session management page showing active sessions with origin and terminate actions.
- **Infrastructure**: EF configuration + migration for the consolidated session table.
- **Tests**: unit tests for session creation/termination semantics; integration tests for token-issuance recording, origin capture, and revocation forcing a 401.
