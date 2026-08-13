## 1. Domain model (Core)

- [ ] 1.1 Extend `UserSession` aggregate (`Core/Modules/UserSessions/Aggregates/UserSession.cs`) with `TenantId`, `ClientId` (nullable), `AuthorizationId` (nullable), `Country` (nullable), `EndedAtUtc` (nullable); add `IsActive` (active ⇔ `EndedAtUtc == null && ExpiresAt > UtcNow`), an `End()` method and an `UpdateLastActivity()` that no longer flips active state; keep existing origin fields (device, sessionToken, ipAddress, userAgent, location).
- [ ] 1.2 Remove the `Session` entity (`Core/Modules/UserSessions/Entities/Session.cs`) and the `Core/Shared/SessionId.cs` value object; update `CreateNewSession` to accept the new fields.

## 2. Persistence

- [ ] 2.1 Update `IUserSessionRepository`/`UserSessionRepository`: add `GetByAuthorizationIdAsync`, `GetActiveSessionsByTenantAsync`, tenant-scoped `GetSessionsByUserIdAsync`, and update `GetActiveSessionsAsync` to use the active semantics.
- [ ] 2.2 Remove `ISessionRepository`/`SessionRepository` and their DI registrations in `InfraestructureExtensions`.
- [ ] 2.3 Update the `UserSession` EF entity configuration for the new columns; add a migration that merges `Sessions` rows into the `UserSessions` table (copying ip/country/started/ended, mapping `AuthorizationId`) and drops the `Sessions` table.

## 3. Application module (consolidated UserSessions)

- [ ] 3.1 Rework `CreateUserSessionCommand`/handler to the consolidated fields (userId, tenantId, clientId, authorizationId, ip, userAgent, device, expiresAt) with origin values.
- [ ] 3.2 Rework `DeactivateUserSessionCommand`/handler to terminate a session by revoking the linked OpenIddict authorization (`FindByIdAsync` + `TryRevokeAsync`) and all its tokens (`RevokeByAuthorizationIdAsync`) via the typed managers, mark the session ended, and support optional `ForceLogout` and `LockUser` flags (force-logout revokes every session of the user; lock additionally locks the account via the existing `LockUserCommand` path).
- [ ] 3.3 Rework `TerminateAllUserSessionsCommand`/handler with the same typed-manager revocation for every session of the user plus the optional `LockUser` flag.
- [ ] 3.4 Update `GetUserSessionsByUserQuery`, `GetAllActiveUserSessionsQuery` and `GetActiveUserSessionsCountQuery`/handlers to return origin fields (ip, userAgent, device, clientId, startedAt, lastActivity, expires, endedAt) and to tenant-scope by `ICurrentTenantContext` when set.
- [ ] 3.5 Delete the `Sessions` Application module (CreateSession/TerminateSession/GetUserSessions commands/queries, `SessionDto`, and the reflection-based `TerminateSessionCommandHandler`).

## 4. Auth flow recording

- [ ] 4.1 Add an origin-capture helper that returns IP (first `X-Forwarded-For` hop, else `Connection.RemoteIpAddress`), raw `User-Agent`, and a parsed device descriptor (with `Unknown` fallback, no new dependency).
- [ ] 4.2 Record a session in `AuthorizationController.Exchange` for both `IsAuthorizationCodeGrantType` and `IsRefreshTokenGrantType` after a successful exchange: subject from `Claims.Subject`, `AuthorizationId` from the `Claims.AuthorizationId` claim, `ClientId` from the request, tenant from `ICurrentTenantContext` (fallback user default), origin from the helper; refresh grants update the existing session by `AuthorizationId` instead of inserting.
- [ ] 4.3 Record a session in `AuthEndpoints` `/auth/login` after the successful admin cookie sign-in with `ClientId="admin"`, no `AuthorizationId`, and origin from the helper.

## 5. Admin API, client services, and UI

- [ ] 5.1 Consolidate admin endpoints under `/api/admin/user-sessions/*`: `POST /{id}/terminate` and `POST /terminate-all/{userId}` accepting `{ forceLogout, lock }`; keep active/by-user/active-count with origin fields; remove `AdminSessionsEndpoints` (`/api/admin/sessions/*`).
- [ ] 5.2 Update `IAdminUserSessionService`/`ServerAdminUserSessionService` and the client `AdminUserSessionService` + `UserSessionModel` to carry origin fields and the terminate operations (with force-logout/lock); remove the `IAdminSessionService`/`AdminSessionService` pair and `SessionModel`.
- [ ] 5.3 Update `Sessions.razor` to merge active sessions and history into one audit view (origin columns: device, ip, user-agent, client, last activity, expires, ended) with Disconnect (`terminate`) and an explicit force-logout + lock action; add/update resource keys in all 8 language files.

## 6. Verification

- [ ] 6.1 Unit tests: session active/inactive transitions (created, ended, expired), terminate revokes authorization + tokens, terminate with `ForceLogout` revokes all sessions, `LockUser` locks the account.
- [ ] 6.2 Integration tests (Server.Tests): session recorded on token issuance with origin captured via `X-Forwarded-For`; terminating a session makes a subsequent refresh return `invalid_grant`; terminate-all revokes every session; admin login records a session.
- [ ] 6.3 Apply the migration on a scratch database, `dotnet build` the solution, and run the test projects touched by this change.
