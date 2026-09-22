## Purpose

Let administrators replace the set of domain permissions assigned to a role from the admin API and console.

## ADDED Requirements

### Requirement: Replace the permission set of a role

The system SHALL expose an admin operation that replaces the complete set of domain permissions assigned to a role with a requested set, adding missing assignments and removing assignments no longer present.

#### Scenario: Permissions added

- **WHEN** a role currently has permissions A and B and the requested set is A, B, C
- **THEN** the role ends with permissions A, B, C

#### Scenario: Permissions removed

- **WHEN** a role currently has permissions A, B and the requested set is A
- **THEN** the role ends with only permission A

#### Scenario: Idempotent replacement

- **WHEN** the requested set equals the role's current set
- **THEN** the operation succeeds without changing the assignments

#### Scenario: Unknown role

- **WHEN** the requested role does not exist
- **THEN** the operation fails with a not-found result

#### Scenario: Unknown permission

- **WHEN** the requested set contains a permission id that does not exist
- **THEN** the operation fails with a validation error and no assignments change

### Requirement: Permission assignment requires admin authorization

The system SHALL restrict the replace-permissions operation to administrators.

#### Scenario: Non-admin caller rejected

- **WHEN** a caller without the admin role invokes the operation
- **THEN** the request is rejected with forbidden

### Requirement: Console manages role permissions

The Blazor admin console SHALL let an administrator view and save the domain permissions assigned to a role.

#### Scenario: Existing assignments shown

- **WHEN** an administrator opens a role that has assigned permissions
- **THEN** the console shows those permissions as selected

#### Scenario: Saving persists the selection

- **WHEN** an administrator changes the selected permissions and saves
- **THEN** the role's assignments are replaced with the selection and the console reports success
