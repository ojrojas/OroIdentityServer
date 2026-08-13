## Purpose

Records every user session with its connection origin (IP, user-agent, device) and lets administrators see active sessions and terminate them, logging a user out of any application by revoking its OpenIddict authorization and tokens.

## ADDED Requirements

### Requirement: Session recorded at token issuance

The system SHALL record a user session when tokens are issued via the authorization-code grant or the refresh-token grant at the token endpoint, capturing the user, the OpenIddict authorization identifier, the client application, and the connection origin.

#### Scenario: Authorization code exchange

- **WHEN** the token endpoint exchanges an authorization code for tokens
- **THEN** a session is recorded for the user with the authorization identifier and connection origin

#### Scenario: Refresh token grant

- **WHEN** the token endpoint redeems a refresh token
- **THEN** the existing session for that authorization is updated (activity refreshed) or a session is recorded if none exists

### Requirement: Session recorded at admin login

The system SHALL record a user session when an administrator signs in through the admin login flow, capturing the user, the client ("admin"), and the connection origin.

#### Scenario: Successful admin sign-in

- **WHEN** an administrator authenticates through the admin login flow
- **THEN** a session is recorded for the administrator with the connection origin

### Requirement: Connection origin captured from the request

A recorded session SHALL include the client IP address (honoring the first `X-Forwarded-For` entry when present, falling back to the direct remote address), the raw `User-Agent` header value, and a device descriptor derived from it.

#### Scenario: Proxy provides X-Forwarded-For

- **WHEN** a request includes an `X-Forwarded-For` header
- **THEN** the session records the first address in that header as the client IP

#### Scenario: Direct connection

- **WHEN** a request arrives without an `X-Forwarded-For` header
- **THEN** the session records the direct remote IP address of the request

#### Scenario: Missing user-agent

- **WHEN** a request has no `User-Agent` header
- **THEN** the session records empty user-agent and an unknown device descriptor without failing

### Requirement: Active session listing

The system SHALL expose the active sessions of a user and, for administrators, all active sessions globally. Each listed session SHALL include the user, tenant, client application, connection origin, device, and start/last-activity time.

#### Scenario: List sessions of a user

- **WHEN** an administrator requests the sessions of a user
- **THEN** the response includes every non-expired, non-revoked session of that user with origin and device details

#### Scenario: List all active sessions

- **WHEN** an administrator requests all active sessions
- **THEN** the response includes every non-expired, non-revoked session across all users with origin and device details

### Requirement: Session considered inactive when expired or revoked

A session SHALL be considered inactive when its authorization has been revoked, when the session has been terminated, or when it has expired.

#### Scenario: Revoked authorization

- **WHEN** the OpenIddict authorization linked to a session is revoked
- **THEN** the session is reported as inactive in any listing

#### Scenario: Expired session

- **WHEN** a session's expiration time has passed
- **THEN** the session is reported as inactive in any listing

### Requirement: Terminate a single session

An administrator SHALL be able to terminate a single session. Termination SHALL revoke the linked OpenIddict authorization and all its tokens (access and refresh) and SHALL mark the session as terminated.

#### Scenario: Terminate an active session

- **WHEN** an administrator terminates a session that is active
- **THEN** the linked authorization and all its tokens are revoked and the session is marked terminated

#### Scenario: Terminate with force-logout and lock

- **WHEN** an administrator terminates a session with the force-logout/lock option enabled
- **THEN** all sessions of the user are revoked and the user account is locked so the user cannot sign in again

#### Scenario: Terminate an already-inactive session

- **WHEN** an administrator terminates a session that is already terminated or expired
- **THEN** the operation succeeds without error and the session remains inactive

### Requirement: Terminate all sessions of a user

An administrator SHALL be able to terminate every session of a user in one operation, revoking each linked authorization and all its tokens and marking the sessions as terminated.

#### Scenario: Terminate all sessions

- **WHEN** an administrator terminates all sessions of a user
- **THEN** every active session of that user is revoked and marked terminated

### Requirement: Remote logout takes effect immediately

After a session is terminated, the client application holding the revoked tokens SHALL be rejected on its next token refresh or API call using those tokens, causing the application to redirect the user to sign in again.

#### Scenario: Client uses revoked refresh token

- **WHEN** a client application attempts to redeem a refresh token whose session was terminated
- **THEN** the token endpoint returns an invalid-grant error and does not issue new tokens

#### Scenario: Client uses revoked access token

- **WHEN** a client application presents an access token whose authorization was revoked
- **THEN** the access token is rejected as invalid

### Requirement: Origin and device details surfaced to administrators

The admin session listing SHALL surface the connection origin (IP, user-agent, device) for each session so administrators can identify where a user is connecting from.

#### Scenario: Admin inspects session origin

- **WHEN** an administrator views the active sessions list
- **THEN** each session shows the recorded IP address, user-agent, and device descriptor
