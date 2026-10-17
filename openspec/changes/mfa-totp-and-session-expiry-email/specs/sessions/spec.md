## Purpose

Aligns the recorded session lifetime with the authentication cookie and makes an expired session
actionable instead of silent.

## MODIFIED Requirements

### Requirement: Sliding session lifetime

The system SHALL keep the recorded session expiry and the authentication cookie in step, so a
session that is still being used does not end at a fixed wall-clock moment, and SHALL renew both
when the session has been idle for at least half of its lifetime.

#### Scenario: Active session

- **WHEN** a session has been idle for less than half of its lifetime
- **THEN** neither the cookie nor the recorded expiry moves

#### Scenario: Half lifetime elapsed

- **WHEN** a session has been idle for at least half of its lifetime
- **THEN** the recorded expiry and the cookie are pushed forward by a full lifetime from now

#### Scenario: Closed session

- **WHEN** a session has already been signed out, revoked or expired
- **THEN** a later request cannot extend it back to life

### Requirement: Actionable expiry

The system SHALL redirect an expired session to the login page carrying the reason and the URL the
user was on, and SHALL answer unauthorized API calls with a location the clients can act on.

#### Scenario: Expired session on a page request

- **WHEN** a request arrives for a session that has expired
- **THEN** the user is redirected to the login page with the expiry reason and the original URL preserved

#### Scenario: Expired session on an API request

- **WHEN** an API request arrives without a valid session
- **THEN** the response is 401 and carries a `Location` pointing at the login page

#### Scenario: Client acts on the 401

- **WHEN** the console's HTTP connector receives a 401 with that location
- **THEN** it navigates the browser to the login page instead of rendering an empty screen

### Requirement: Session culture recorded

The system SHALL record the culture a session was started with, so later notifications can be
produced without a request context.

#### Scenario: Session created

- **WHEN** a session is opened from a request with a resolved culture
- **THEN** that culture is stored with the session