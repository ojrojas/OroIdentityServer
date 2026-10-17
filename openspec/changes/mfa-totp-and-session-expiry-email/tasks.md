## 1. Prerequisites

- [x] 1.1 Enable and persist Data Protection (`AddDataProtection().PersistKeysToFileSystem(...).SetApplicationName(...)`) so cookies and encrypted TOTP secrets survive restarts and replicas.
- [x] 1.2 Give each cookie scheme its own key purpose (`PurposeDataProtectionProvider`) so a pending-MFA ticket can never be read by the admin scheme.
- [x] 1.3 Introduce a single `CookieAuthHandlerSetup.SessionLifetime` shared by the cookie, the `UserSession` row and the sliding refresh.
- [x] 1.4 Make `UserSession.ExtendsExpiry` slide with the cookie from the session-validation middleware.
- [x] 1.5 Carry `reason=expired` and the original `ReturnUrl` on the expiry redirect; add the banner to `Login.razor` and the keys to all 8 locales.
- [x] 1.6 Answer `/api/*` 401s with a `Location` header and add the `UnauthorizedRedirectHandler` to the console client.

## 2. MFA domain

- [x] 2.1 Add the `Core/Modules/Mfa` module: `MfaCredential`, `MfaRecoveryCode`, `MfaLoginChallenge`, value objects, `MfaChallengePurpose`/`MfaChallengeStatus` enums and domain events.
- [x] 2.2 Add `ITotpService`/`ITotpService` implementation with Otp.NET (base32 secret, otpauth URI, ±1 step window, matched step out-parameter).
- [x] 2.3 Add `IMfaSecretProtector` (Data Protection) and `IRecoveryCodeGenerator` (cryptographic RNG, BCrypt hashes, normalisation).
- [x] 2.4 Add `IQrCodeGenerator` (QRCoder PNG).
- [x] 2.5 Extend `UserSession` with `EndReason`, `EndedAtUtc`, `ExpiryNotifiedAtUtc`, `Culture`, `ExtendExpiry`, `MarkExpired`, `MarkExpiryNotified`; add `RequireMfa` + `UpdateMfaPolicy` to `Tenant`.

## 3. Persistence

- [x] 3.1 Add the three entity configurations, `DbSet`s, `ApplyConfiguration` registrations and the three repositories with explicit DI.
- [x] 3.2 Generate the `AddMfaAndSessionExpiry` migration (three tables, five columns, indexes).
- [x] 3.3 Add the projected `GetExpiredPendingNotificationAsync` (two translatable queries instead of a join on converted ids).

## 4. Application module

- [x] 4.1 Commands: `StartMfaEnrollment`, `ConfirmMfaEnrollment`, `DisableMfa`, `RegenerateMfaRecoveryCodes`, `ResetMfa`, `CreateMfaLoginChallenge`, `VerifyMfaChallenge`, `CompleteMfaEnrollmentChallenge`.
- [x] 4.2 Queries: `GetMfaStatus`, `IsMfaRequired`, `GetMfaChallenge`, `GetMfaEnrollmentSecret`.
- [x] 4.3 `ProcessExpiredSessionNotifications` command: close, notify, stamp — idempotent and resilient to transport failures.

## 5. Auth flow

- [x] 5.1 Register the pending-MFA cookie scheme and its read/start/clear helpers.
- [x] 5.2 Split `/auth/login`: password OK + MFA due ⇒ challenge and redirect; otherwise the previous single-step behaviour.
- [x] 5.3 Extract `CompleteSignInAsync` so both entry points write the cookie, open the session and resolve the redirect identically (including `SanitizeReturnUrl`).
- [x] 5.4 Add `/auth/mfa`, `/auth/mfa/enroll` and `/auth/mfa/qr`; clear the pending cookie on success.
- [x] 5.5 Add `/Account/Mfa` and `/Account/MfaSetup` (static SSR, server-rendered QR and manual key).

## 6. Self-service API and console UI

- [x] 6.1 `/api/me/mfa` group (status, setup, confirm, disable, recovery codes) plus `/api/users/{id}/mfa/reset`.
- [x] 6.2 `IAdminMfaService` (HTTP) and `ServerAdminMfaService` (CQRS, same contract).
- [x] 6.3 `/account/security/mfa` page, nav entry, page title and admin reset action in the user detail page.
- [x] 6.4 Tenant `RequireMfa` switch plumbed through command, DTO, models, service and page.

## 7. Email

- [x] 7.1 `IEmailSender`, `EmailMessage` and `EmailOptions`.
- [x] 7.2 `FileSystemEmailSender` writing the body plus a JSON envelope, with `Enabled` honoured.
- [x] 7.3 `SessionExpiryEmailComposer` using the session culture.
- [x] 7.4 `ExpiredSessionNotificationJob` registered in Quartz (every 5 minutes, `DisallowConcurrentExecution`).

## 8. Resources, docs and tests

- [x] 8.1 Add all MFA / session-expiry keys to the 8 `SharedResources.*.resx` files.
- [x] 8.2 README: features, auth endpoints, `/api/me/mfa`, email configuration, static pages; `docs/authorization.md`: what clients see and troubleshooting.
- [x] 8.3 Unit tests: TOTP windows/replay, challenge attempt budget, credential transitions, recovery codes, secret protection, enrolment guard, expiry sweep semantics, email/QR transports, localized composer.
- [x] 8.4 Integration tests: login challenge without session, wrong code, valid code, lockout, recovery code, page reload, QR guard, tenant-policy enrolment (accepted and rejected), self-service auth requirements, expiry redirect with reason, 401 `Location`.