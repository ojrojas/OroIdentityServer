## Purpose

Let administrators manage the domain permission catalogue (list, create, edit, delete) from the console, with system permissions protected.

## ADDED Requirements

### Requirement: Permission catalogue list

The console SHALL show the domain permissions with their name (`Provider.Resource.Action`), description and whether they are system permissions, and SHALL offer a create action.

#### Scenario: Catalogue displayed

- **WHEN** an administrator opens the permissions page
- **THEN** the page lists the permissions with their name, description and system flag

#### Scenario: Empty catalogue

- **WHEN** the catalogue has no permissions
- **THEN** the page shows an empty-state message instead of a table

### Requirement: Create a permission

The console SHALL let an administrator create a permission from provider, resource, action and description.

#### Scenario: Permission created

- **WHEN** an administrator submits a new permission with a non-empty action
- **THEN** the permission is created and appears in the catalogue

#### Scenario: Invalid permission rejected

- **WHEN** an administrator submits a permission with an empty action
- **THEN** the request fails and the console reports the failure

### Requirement: Edit a permission

The console SHALL let an administrator edit a non-system permission.

#### Scenario: Non-system permission edited

- **WHEN** an administrator changes a non-system permission and saves
- **THEN** the change is persisted and reported as success

### Requirement: System permissions are protected

The console SHALL prevent editing and deleting system permissions.

#### Scenario: System permission read-only

- **WHEN** an administrator opens a system permission
- **THEN** its fields are read-only and the delete action is not available

#### Scenario: Delete a non-system permission

- **WHEN** an administrator deletes a non-system permission
- **THEN** the permission is removed from the catalogue

### Requirement: Menu entry and gating

The console navigation SHALL include a permissions entry visible to administrators.

#### Scenario: Administrator sees the entry

- **WHEN** a user with the Admin or Administrator role is signed in
- **THEN** the permissions navigation entry is visible

#### Scenario: Non-administrator does not see the entry

- **WHEN** a user without the Admin or Administrator role is signed in
- **THEN** the permissions navigation entry is not rendered
