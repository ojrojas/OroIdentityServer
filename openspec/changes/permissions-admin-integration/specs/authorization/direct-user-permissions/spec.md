## Purpose

Allow granting domain permissions directly to a user, in addition to the permissions the user inherits from their roles.

## ADDED Requirements

### Requirement: Direct user permissions

The system SHALL support permissions granted directly to a user, independent of roles.

#### Scenario: Direct permission granted

- **WHEN** a permission is assigned directly to a user
- **THEN** the assignment is persisted for that user

#### Scenario: Direct permission removed

- **WHEN** a direct permission is removed from a user
- **THEN** the assignment no longer exists for that user

### Requirement: Effective permissions combine roles and direct grants

A user's effective permission names SHALL be the distinct union of the permissions inherited from their active roles and the permissions granted directly to the user.

#### Scenario: Union of role and direct permissions

- **WHEN** a user inherits permission A from a role and is granted permission B directly
- **THEN** the resolved effective permissions contain both A and B

#### Scenario: Duplicate across sources

- **WHEN** a user inherits permission A from a role and is also granted A directly
- **THEN** the resolved effective permissions contain A only once

#### Scenario: Deactivated role ignored

- **WHEN** a role granting permission A is deactivated
- **THEN** A is not resolved from that role (a direct grant of A would still apply)

### Requirement: Effective permissions reach tokens and userinfo

Directly granted permissions SHALL be emitted through the same claim contract as role-derived permissions.

#### Scenario: Direct permission in the access token

- **WHEN** an access token is issued for a user with a directly granted permission
- **THEN** the token contains a `permission` claim for that permission

#### Scenario: Direct permission in userinfo

- **WHEN** `userinfo` is called with the `permissions` scope for a user with a directly granted permission
- **THEN** the response lists that permission

### Requirement: Replace the direct permission set of a user

The system SHALL expose an admin operation that replaces the complete set of permissions granted directly to a user.

#### Scenario: Replace set

- **WHEN** a user has direct permissions A and B and the requested set is B and C
- **THEN** the user's direct permissions become B and C

#### Scenario: Unknown user

- **WHEN** the requested user does not exist
- **THEN** the operation fails with a not-found result

#### Scenario: Unknown permission

- **WHEN** the requested set contains an unknown permission id
- **THEN** the operation fails with a validation error and no assignment changes

### Requirement: Console shows direct and effective permissions

The user detail page SHALL let an administrator edit the user's direct permissions and SHALL show the user's effective permissions.

#### Scenario: Direct permissions shown as selected

- **WHEN** an administrator opens a user with direct permissions
- **THEN** the direct-permission checklist shows those permissions as selected

#### Scenario: Effective permissions shown

- **WHEN** an administrator opens a user who inherits permissions from roles and holds direct permissions
- **THEN** the effective-permissions panel lists the union of both
