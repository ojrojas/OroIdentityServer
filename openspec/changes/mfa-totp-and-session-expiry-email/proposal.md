## Why

Two gaps in the sign-in experience, both visible to end users:

- The login flow is a single step. A password is enough to obtain an admin session, and a leaked
  password is enough to walk into the console of every application registered against this server.
  There is no authenticator-app (TOTP) factor anywhere in the codebase.
- When a session times out, the user is bounced to a bare login page: the redirect discards the
  URL they were on, the page gives no reason, and the `/api/*` endpoints answer `401` with no
  `Location`, so the SPA degrades instead of returning the user to the login screen. Nothing is
  ever told to the user out of band, even though a timeout is exactly the event worth notifying.

## What Changes

- **Second factor at login.** After the password is accepted, a pending challenge is created and
  the user is redirected to `/Account/Mfa`. Only a valid authenticator code — or a single-use
  recovery code — issues the `oro.identity.admin` cookie and opens the `UserSession`. The same
  cookie backs RP-initiated `connect/authorize`, so relying-party logins go through the identical
  path. Adds `MfaCredential`, `MfaRecoveryCode` and `MfaLoginChallenge` aggregates, a
  `/account/security/mfa` console page, `/api/me/mfa` self-service endpoints, and static
  `/Account/Mfa` / `/Account/MfaSetup` pages. **BREAKING for scripted logins** of users with a
  factor enrolled: they must now submit a second POST to `/auth/mfa`.
- **Opt-in per user, enforceable per tenant.** A user manages their own factor at
  `/account/security/mfa`; `Tenant.RequireMfa` forces every member of a tenant to enrol, routing
  users without a credential through setup instead of locking them out. Administrators can reset a
  user's factor from the user detail page.
- **Attack limits on the challenge.** The attempt budget lives server-side (5 attempts), TOTP
  codes cannot be replayed inside a 30-second step, each challenge is single-use and expires after
  5 minutes, and the shared secret is encrypted at rest with Data Protection.
- **Session-expiry email.** A Quartz sweep every 5 minutes closes sessions that ran out of time
  and queues one localized notification per session, sent in the culture the session was started
  in. Explicit sign-outs and administrator revocations are recorded with a distinct `EndReason` so
  they never masquerade as expiries. Transport is pluggable (`IEmailSender`); the default `File`
  sender writes the rendered message to disk.
- **Coherent session lifetime.** The `UserSession` expiry now slides with the authentication
  cookie (shared 8-hour constant), so "expired" means "idle for 8 hours" instead of "the wall
  clock reached login + 8h". Data Protection is now persisted to `data-protection-keys/`, which is
  what makes both cookies and the encrypted TOTP secrets survive a restart.
- **Actionable expiry UX.** The expiry redirect carries `reason=expired` and the original
  `ReturnUrl`, the login page explains what happened, `/api/*` 401s carry a `Location` header, and
  the console's HTTP connectors turn that header into a navigation back to the login page.

## Capabilities

### New Capabilities

- `mfa/authenticator-factor`: The system SHALL support a time-based one-time-password second
  factor at the interactive login, with self-service enrolment, recovery codes, an optional
  tenant-wide policy, bounded attempts, replay protection, and an administrator reset.
- `notifications/session-expiry-email`: The system SHALL notify the user by email when their
  session expires from inactivity, exactly once per session, in the language of that session.

### Modified Capabilities

- `sessions/lifetime`: The database session now slides together with the authentication cookie, and
  records why it ended so an expiry can be told apart from a sign-out or a revocation.
- `sessions/expiry-ux`: An expired session redirects to the login page carrying the reason and the
  original URL, and answers API calls with a `Location` the clients can act on.

## Impact

- **Core**: new `Mfa` module (aggregates, value objects, enums, repository interfaces, domain
  events); `UserSession` gains `EndReason`, `EndedAtUtc`, `ExpiryNotifiedAtUtc`, `Culture`,
  `ExtendExpiry`, `MarkExpired`, `MarkExpiryNotified`; `Tenant` gains `RequireMfa`; new service
  interfaces (`ITotpService`, `IMfaSecretProtector`, `IQrCodeGenerator`, `IRecoveryCodeGenerator`,
  `IEmailSender`, `ISessionExpiryEmailComposer`).
- **Infrastructure**: three new tables and a migration (`AddMfaAndSessionExpiry`), three
  repositories, `TotpService` (Otp.NET), `QrCodeGenerator` (QRCoder), `FileSystemEmailSender`, and
  a projected query for the expiry sweep.
- **Application**: new `Mfa` CQRS module (enrolment, confirm, disable, regenerate recovery codes,
  reset, challenge create/verify/complete, status and requirement queries) and
  `ProcessExpiredSessionNotificationsCommand`.
- **IdentityServer**: Data Protection enabled; a second cookie scheme for the pending challenge
  with its own key purpose; `/auth/login` split into two steps; `/auth/mfa`, `/auth/mfa/enroll`,
  `/auth/mfa/qr`; `/api/me/mfa` and `/api/users/{id}/mfa/reset`; `/Account/Mfa` and
  `/Account/MfaSetup`; the session-expiry Quartz job; the login/middleware expiry changes.
- **IdentityServer.Client**: `/account/security/mfa` page with nav entry, `IAdminMfaService`
  (HTTP) and `ServerAdminMfaService` (CQRS), an unauthorized-redirect handler, the tenant policy
  switch, and the administrator reset action.
- **Tests**: 147 unit tests (TOTP windows and replay, challenge attempt budget, recovery codes,
  secret protection, expiry sweep idempotency, email and QR transports, localized composer) and 14
  new integration tests covering the whole login challenge flow.
- **Docs**: README (features, endpoints, configuration), `docs/authorization.md` (what a client
  application sees when MFA is on, plus troubleshooting).