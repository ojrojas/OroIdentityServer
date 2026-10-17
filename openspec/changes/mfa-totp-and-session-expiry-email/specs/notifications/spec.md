## Purpose

Notifies the user by email when a session ends because it ran out of time, so a timeout is
explained instead of silently returning them to a bare login page, and gives the session record
enough information to tell a timeout apart from a deliberate sign-out.

## ADDED Requirements

### Requirement: Expiry notification

The system SHALL send the user one email when a session expires from inactivity, and SHALL record
on the session that the notification was sent so it is never repeated.

#### Scenario: Session times out

- **WHEN** a session passes its expiry without enough activity to keep it alive
- **THEN** the session is closed as expired and a single email is queued for the user

#### Scenario: Sweep runs again

- **WHEN** the sweep runs after a session has already been notified
- **THEN** no second email is sent for that session

#### Scenario: No address on file

- **WHEN** an expired session belongs to a user without an email address
- **THEN** the session is still closed and no email is sent

#### Scenario: Transport failure

- **WHEN** the email transport fails for an expired session
- **THEN** the session is still closed and marked as notified, and the failure does not stop the sweep

### Requirement: Notification content and language

The email SHALL identify the application, the moment the session ended and the device, and SHALL be
rendered in the culture the session was started in.

#### Scenario: Localized delivery

- **WHEN** a session was started with a supported culture
- **THEN** the subject and body are rendered in that language, falling back to the neutral language when it is unsupported

### Requirement: Notification after the fact

The email SHALL be queued by a scheduled sweep rather than by the request that detects the expiry.

#### Scenario: Browser already gone

- **WHEN** a session expires and no further request ever arrives for it
- **THEN** the notification is still sent by the scheduled sweep

## MODIFIED Requirements

### Requirement: Distinguishable session end reasons

The system SHALL record why a session ended, so that a timeout can be told apart from a voluntary
sign-out or an administrator revocation.

#### Scenario: User signs out

- **WHEN** a user signs out explicitly
- **THEN** the session is closed as a sign-out and no expiry notification is produced

#### Scenario: Administrator terminates sessions

- **WHEN** an administrator terminates the sessions of a user
- **THEN** the sessions are closed as revoked by an administrator and no expiry notification is produced

### Requirement: Pluggable transport

The system SHALL resolve the outbound email transport by configuration and SHALL support disabling
delivery altogether.

#### Scenario: Delivery disabled

- **WHEN** email is disabled
- **THEN** messages are dropped instead of being dispatched, and the flow that produced them continues