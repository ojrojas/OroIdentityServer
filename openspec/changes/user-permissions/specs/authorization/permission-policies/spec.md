## Purpose

Enforce domain user permissions as ASP.NET Core authorization policies that match the `permission` claim exactly, so endpoints and UI can require a specific permission by name.

## ADDED Requirements

### Requirement: Exact-match permission policies

The system SHALL evaluate a policy named `perm:<permission-name>` as satisfied only when the authenticated principal holds a `permission` claim whose value is exactly `<permission-name>`.

#### Scenario: User holds the required permission

- **WHEN** a request targets an endpoint protected by `perm:oropos.sales.read` and the principal has a `permission` claim with value `oropos.sales.read`
- **THEN** authorization succeeds

#### Scenario: User lacks the required permission

- **WHEN** a request targets an endpoint protected by `perm:oropos.sales.read` and the principal has no `permission` claim with that exact value
- **THEN** authorization fails with forbidden

#### Scenario: Matching is exact, not a pattern

- **WHEN** the principal holds `permission` = `oropos.sales.write` and the policy requires `perm:oropos.sales.read`
- **THEN** authorization fails, because permission matching does not use wildcards or prefixes

#### Scenario: Anonymous principal

- **WHEN** an unauthenticated request targets a `perm:<name>` policy
- **THEN** authorization fails with unauthorized

### Requirement: Policies are created on demand

The system SHALL resolve `perm:<permission-name>` policies without requiring every permission to be registered at application startup, so permissions created at runtime are immediately usable.

#### Scenario: Newly created permission policy resolves

- **WHEN** a permission is created through the admin API and an endpoint is protected with its `perm:<name>` policy
- **THEN** the policy is resolved and evaluated without an application restart

### Requirement: Client-side permission gating

The Blazor admin console SHALL expose a component that renders its content only when the current authenticated user holds a given permission.

#### Scenario: Content rendered when permission held

- **WHEN** the signed-in user holds the required permission
- **THEN** the component renders its child content

#### Scenario: Content hidden when permission missing

- **WHEN** the signed-in user does not hold the required permission
- **THEN** the component does not render its child content
