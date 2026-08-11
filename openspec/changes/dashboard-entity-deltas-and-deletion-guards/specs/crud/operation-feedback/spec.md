## Purpose

Every admin CRUD operation surfaces clear success or failure feedback to the user through toasts, so users always know whether an action completed.

## ADDED Requirements

### Requirement: Success toast on every successful write operation

Every create, update, delete, and soft-delete operation on admin-managed entities SHALL display a success toast when the operation succeeds. Covered entity forms include, at minimum: users, roles, tenants, identification types, applications, scopes, and user sessions.

#### Scenario: Create succeeds

- **WHEN** an operator submits a create form and the API returns success
- **THEN** the UI shows a success toast for that entity

#### Scenario: Update succeeds

- **WHEN** an operator saves an edit form and the API returns success
- **THEN** the UI shows a success toast for that entity

#### Scenario: Delete and soft-delete succeed

- **WHEN** an operator confirms a delete or soft-delete and the API returns success
- **THEN** the UI shows a success toast for that entity

### Requirement: Failure toast on every failed write operation

Every create, update, delete, and soft-delete operation SHALL display a failure toast that includes the returned HTTP status code when the API returns a failure.

#### Scenario: Create fails

- **WHEN** a create request returns a failure status
- **THEN** the UI shows a failure toast including the HTTP status code

#### Scenario: Deletion rejected by a guard

- **WHEN** a delete or soft-delete request returns a conflict (409)
- **THEN** the UI shows a failure toast including the status code and the rejection reason

### Requirement: No silent failures

The UI SHALL NOT leave a failed or successful write operation without any toast feedback.

#### Scenario: Failed operation always surfaces feedback

- **WHEN** any write operation completes with a non-success status
- **THEN** a failure toast is always displayed
