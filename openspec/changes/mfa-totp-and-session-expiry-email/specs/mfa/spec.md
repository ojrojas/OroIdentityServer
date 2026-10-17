## Purpose

Adds a time-based one-time-password (TOTP) second factor to the interactive login, so that a
leaked password alone is not enough to obtain an admin session, with self-service enrolment,
recovery codes, an optional tenant-wide policy and administrator recovery.

## ADDED Requirements

### Requirement: Second factor required after a valid password

The system SHALL require a second factor at the interactive login when the user has an enrolled
authenticator or when the user's tenant mandates one, and SHALL NOT issue the authentication
cookie, the session, or any token until the factor is cleared.

#### Scenario: Password accepted, factor enrolled

- **WHEN** a user with an enrolled authenticator submits valid credentials at the login endpoint
- **THEN** no authentication cookie is issued and no session is recorded
- **AND** the browser is redirected to the server-owned second-factor page

#### Scenario: Factor cleared

- **WHEN** the user submits a valid code for that challenge
- **THEN** the authentication cookie is issued, the session is recorded and the original return URL is honoured

#### Scenario: Relying-party login

- **WHEN** a client application starts an authorization request for a user whose session must be authenticated
- **THEN** the user goes through the same password-then-factor interaction, with no client-visible protocol change

### Requirement: Self-service enrolment

The system SHALL let an authenticated user enrol an authenticator app from the console and SHALL
keep the factor inactive until the first code is confirmed.

#### Scenario: Start enrolment

- **WHEN** the user starts enrolment
- **THEN** a new secret is generated, stored encrypted at rest, and returned as an `otpauth://` URI, a manual key and a QR code

#### Scenario: Confirm enrolment

- **WHEN** the user submits the code shown by the authenticator
- **THEN** the factor becomes active, the consumed time step is recorded, and ten single-use recovery codes are issued and shown once

#### Scenario: Enrolment refused while a factor is active

- **WHEN** enrolment is requested for a user who already has an active factor
- **THEN** the request is refused and the existing factor keeps working

### Requirement: Recovery codes

The system SHALL issue single-use recovery codes that can stand in for an authenticator code, and
SHALL store only their hashes.

#### Scenario: Signing in with a recovery code

- **WHEN** a user submits an unused recovery code at the challenge
- **THEN** the challenge is cleared and the code is marked as used

#### Scenario: Reusing a recovery code

- **WHEN** a user submits a recovery code that was already used
- **THEN** it is refused and counted as a failed attempt

### Requirement: Turning the factor off

The system SHALL require both the account password and a current code to disable a factor, and
SHALL delete the recovery codes when it is disabled.

#### Scenario: Disabling with the wrong password

- **WHEN** a user submits a valid code but the wrong password
- **THEN** the factor remains active

### Requirement: Tenant policy

The system SHALL support a tenant-wide policy that requires the factor for every member, and SHALL
route members without an enrolled factor through enrolment instead of refusing their sign-in.

#### Scenario: Policy on, factor not enrolled

- **WHEN** a member of a tenant with the policy enabled signs in with valid credentials
- **THEN** the browser is redirected to the enrolment step, and no session is issued before the first code is confirmed

### Requirement: Bounded attempts and replay protection

The system SHALL limit the number of code submissions per challenge and SHALL refuse a code whose
time step has already been consumed.

#### Scenario: Attempt budget exhausted

- **WHEN** five incorrect codes are submitted for one challenge
- **THEN** the challenge is locked and further submissions are refused, including a correct code, until the user authenticates again

#### Scenario: Replayed code

- **WHEN** a code from an already-consumed time step is submitted
- **THEN** it is refused and counted as a failed attempt

#### Scenario: Expired challenge

- **WHEN** a challenge is older than its validity window
- **THEN** it can no longer be completed

### Requirement: Administrator reset

The system SHALL let an administrator remove another user's factor without knowing their code or
password, so that a user who lost the device can sign in again.

#### Scenario: Reset performed

- **WHEN** an administrator resets a user's factor
- **THEN** the factor and its recovery codes are removed and the next sign-in enrols again