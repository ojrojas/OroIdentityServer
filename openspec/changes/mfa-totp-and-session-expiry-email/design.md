# Design — MFA (TOTP) and session-expiry email

## Context

The login flow had exactly one door: `POST /auth/login` verified the password, wrote the
`oro.identity.admin` cookie and opened the `UserSession`. That same cookie is what
`/connect/authorize` accepts, so every relying-party login shares it.

## Decisions

### D1 — The challenge lives in the database, not only in the cookie

A six-digit TOTP code has ~10^6 possible values and codes are valid for up to three 30-second
steps (now ±1 for clock drift). An attempt budget enforced from the browser is not an attempt
budget: a client holding an old copy of a pending cookie would replay an expired counter. So the
pending state is a `MfaLoginChallenge` row — token, purpose, `ExpiresAtUtc`, `Attempts`, `Status`
— and the cookie (`oro.identity.mfa.pending`, its own scheme, 5 minutes) only carries the opaque
token that points at it.

Rejected alternative: `ITicketStore` + ASP.NET session. Same trust problem, and it adds a session
store to an app that deliberately has none.

### D2 — Two cookie schemes, two key purposes

`.NET 10` removed `CookieAuthenticationOptions.DataProtectionPolicy`, so scheme separation is done
by handing each scheme a `PurposeDataProtectionProvider` (`oro.identity.admin.v1` /
`oro.identity.mfa.pending.v1`). The purpose is part of the derived key, so a pending ticket cannot
be unprotected by the admin scheme even though both tickets are readable cookies.

### D3 — Replay protection is per time step, and enrolment burns its own step

`MfaCredential.LastUsedTimeStep` stores the highest consumed step; a code whose matched step is not
greater is refused. `Confirm` sets it to the enrolment step, so the code used to enrol cannot be
reused as a login code — the user waits for the next one. This is visible behaviour and is called
out in `docs/authorization.md` troubleshooting.

### D4 — The expiry sweep is Quartz, not request-driven

The request that notices the expiry is, by definition, a request from a browser that may already be
gone. `ExpiredSessionNotificationJob` runs every 5 minutes (and once at startup), dispatches
`ProcessExpiredSessionNotificationsCommand`, and stamps `ExpiryNotifiedAtUtc` in the same save that
closes the session, so a crash mid-send cannot produce a second mail.

### D5 — `SessionEndReason` exists because `ExpiresAt = UtcNow` was ambiguous

`DeactivateSession()` set `ExpiresAt = UtcNow` for every kind of end. Without a reason column,
every voluntary sign-out would look like an idle timeout and generate an email. `SessionEndReason`
(`None`/`Expired`/`LoggedOut`/`RevokedByAdmin`) plus `EndedAtUtc` makes the sweep's predicate exact.

### D6 — The database session slides with the cookie

`ExpiresAt` never moved while the cookie slid on `SlidingExpiration`, producing a guaranteed cut at
login + 8h. Both now use `CookieAuthHandlerSetup.SessionLifetime`, and the middleware extends the
row at the same half-life threshold the cookie uses. "Expired" now means idle.

### D7 — Language comes from the session

A Quartz job has no request and therefore no `RequestLocalization`. `UserSession.Culture` records
the culture at login; `SessionExpiryEmailComposer` applies it for the resource lookup and restores
the thread culture afterwards (there is no per-call culture overload on `IStringLocalizer` in
.NET 10).

### D8 — Self-service lives outside the `/api` group

`/api` requires `ManagerOrAdmin`; a plain `Member` must still be able to manage their own factor, so
`/api/me/*` is a separate group with only `RequireAuthorization()`, and the user id always comes
from the session.

### D9 — TOTP and QR as dependencies, not hand-rolled

`Otp.NET` (`Totp`, `Base32Encoding`) and `QRCoder` are used instead of a hand-written RFC 6238
implementation: the QR code in particular is a real encoder, and the login path should not carry
that risk. Both sit behind `ITotpService` / `IQrCodeGenerator` declared in Core, so the domain has
no package dependency and the verification is unit-testable with a fixed instant (the library's
`VerifyTotp(code, ...)` overload reads the machine clock and is therefore unusable in tests).

## Risks

| Risk | Mitigation |
|---|---|
| Persisted Data Protection keys are lost | Sessions and enrolled authenticators become unusable. Documented in `docs/authorization.md`; the container setups already mount the directory. |
| Enrolling a factor invalidates a previous one | Rotation is explicit, and starting enrolment on a working factor is refused with 409. |
| Tenant policy without enrolment would lock users out | The login routes unenrolled members through `/Account/MfaSetup` instead of refusing them. |
| A user who loses the phone cannot self-serve | Administrator reset (`POST /api/users/{id}/mfa/reset`) plus ten single-use recovery codes. |