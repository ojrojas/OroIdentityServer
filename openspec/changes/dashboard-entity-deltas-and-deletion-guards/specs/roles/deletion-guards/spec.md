## Purpose

Roles with existing associations cannot be deleted, preventing dangling references that would break user role assignments and role-based authorization.

## ADDED Requirements

### Requirement: Role with assigned users cannot be deleted

The system SHALL reject the deletion (soft-delete) of a role that has any associated `UserRole` records. The API SHALL return a `Conflict` result (HTTP 409) with a clear message stating that the role is still assigned to users.

#### Scenario: Delete role assigned to a user

- **WHEN** a delete request targets a role that has at least one `UserRole` record
- **THEN** the system returns HTTP 409 with a message indicating the role cannot be deleted because it is assigned to users

#### Scenario: Role not assigned to users can proceed

- **WHEN** a delete request targets a role with no `UserRole` records and no `RolePermission` records
- **THEN** the system soft-deletes the role and returns success

### Requirement: Role with role permissions cannot be deleted

The system SHALL reject the deletion (soft-delete) of a role that has any associated `RolePermission` records. The API SHALL return a `Conflict` result (HTTP 409) with a clear message stating that the role still has permissions assigned.

#### Scenario: Delete role that holds permissions

- **WHEN** a delete request targets a role that has at least one `RolePermission` record
- **THEN** the system returns HTTP 409 with a message indicating the role cannot be deleted because it has permissions assigned

### Requirement: Deletion check evaluates all associations

The deletion guard SHALL evaluate both `UserRole` and `RolePermission` associations before deleting a role; a role SHALL only be deleted when both checks pass.

#### Scenario: Role fails one of the two guards

- **WHEN** a role has a `RolePermission` but no `UserRole` records (or vice versa)
- **THEN** the system still rejects the deletion with HTTP 409 and a clear message

### Requirement: Guard behavior is enforced at the API layer

The deletion guard SHALL be enforced server-side by the delete-role command, so the UI cannot bypass it and direct API callers receive the same conflict result.

#### Scenario: Direct API call with associated role

- **WHEN** a caller invokes the delete-role API for a role that has users or permissions associated
- **THEN** the API returns HTTP 409 regardless of how the request was issued
