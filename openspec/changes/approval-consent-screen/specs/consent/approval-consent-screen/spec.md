## Purpose

Shows an interactive approval screen when a client application is configured with explicit consent (or requests `prompt=consent`), letting the signed-in user approve or deny the requested scopes before tokens are issued.

## ADDED Requirements

### Requirement: Approval screen shown for explicit consent

The authorization endpoint SHALL display an interactive approval screen when the calling application's consent type is explicit and no valid permanent authorization covers the requested scopes for the signed-in user, unless the client sent `prompt=none`.

#### Scenario: Explicit consent without prior authorization

- **WHEN** an authenticated user starts an authorization request for an application with explicit consent and no prior permanent authorization matches the requested scopes
- **THEN** the server responds with the approval screen instead of issuing tokens

#### Scenario: Explicit consent with a matching permanent authorization

- **WHEN** an authenticated user starts an authorization request for an application with explicit consent and a valid permanent authorization already covers the requested scopes, and the client did not send `prompt=consent`
- **THEN** the server skips the approval screen and completes the authorization request directly

#### Scenario: Prompt equals consent re-displays the screen

- **WHEN** a client sends `prompt=consent` for an application with explicit consent even though a matching permanent authorization exists
- **THEN** the server displays the approval screen again for the user to re-approve

#### Scenario: Prompt equals none with no authorization

- **WHEN** a client sends `prompt=none` and no matching permanent authorization exists
- **THEN** the server returns a consent-required error instead of displaying the approval screen

### Requirement: Approval screen shows application and requested scopes

The approval screen SHALL identify the calling client application by its registered display name and SHALL list every requested scope with its registered name and localized display name or description when available.

#### Scenario: Registered application name displayed

- **WHEN** the approval screen is rendered for a client application that has a registered display name
- **THEN** the screen shows that display name to the user

#### Scenario: Registered scopes with localized descriptions

- **WHEN** a requested scope is registered with OpenIddict and has a localized display name or description
- **THEN** the screen lists the scope with that localized text

#### Scenario: Unregistered scope name fallback

- **WHEN** a requested scope is not registered or has no display metadata
- **THEN** the screen still lists the scope using its raw scope name

### Requirement: Request parameters preserved through the approval screen

The approval screen SHALL carry the full original authorization request back to the authorization endpoint when the user decides, so the authorization completes with the exact `client_id`, `redirect_uri`, `response_type`, `scope`, `state`, `nonce`, `code_challenge`, and any other parameters the client originally sent.

#### Scenario: State and nonce survive the round-trip

- **WHEN** the user approves an authorization request that included `state` and `nonce`
- **THEN** the completed authorization response is delivered to the client with those same `state` and `nonce` values

#### Scenario: PKCE parameters survive the round-trip

- **WHEN** the user approves an authorization request that included `code_challenge` and `code_challenge_method`
- **THEN** the authorization code is issued for the original challenge and exchanged successfully with the matching verifier

### Requirement: Approval completes the authorization

The approval screen SHALL offer an explicit approve action. When the user approves, the server SHALL issue the authorization response for the approved scopes and SHALL create a permanent authorization for the user and client so future requests with the same scopes do not ask again.

#### Scenario: User approves

- **WHEN** the user approves the requested scopes
- **THEN** the server completes the authorization request and redirects to the client's redirect URI with the authorization code and the original `state`

#### Scenario: Permanent authorization created on approval

- **WHEN** the user approves and no permanent authorization existed before
- **THEN** a permanent authorization linking the user, the client, and the approved scopes is stored

### Requirement: Denial aborts the authorization

The approval screen SHALL offer an explicit deny action. When the user denies, the server SHALL abort the authorization request with an `access_denied` error delivered to the client using the request's response mode.

#### Scenario: User denies

- **WHEN** the user denies the requested scopes
- **THEN** the server redirects to the client's redirect URI with an `access_denied` error and the original `state`, and no authorization is created

### Requirement: Approval screen protected against cross-site requests

The approve and deny actions SHALL be protected with antiforgery validation so a third-party site cannot submit a consent decision on the user's behalf.

#### Scenario: Submit without a valid antiforgery token

- **WHEN** a consent decision is submitted without a valid antiforgery token
- **THEN** the server rejects the submission and does not complete or abort the authorization
