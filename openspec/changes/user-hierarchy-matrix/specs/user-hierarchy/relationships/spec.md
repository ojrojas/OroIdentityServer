## Purpose

Gestiona las relaciones de reporte entre usuarios (quién reporta a quién) con soporte para múltiples superiores, tipos de relación y prioridades.

## ADDED Requirements

### Requirement: Create reporting relationship
The system SHALL allow creating a reporting relationship between two users within the same tenant with a specified type and priority.

#### Scenario: Create functional relationship
- **WHEN** a user with appropriate permissions creates a relationship with type "Functional" and priority 1
- **THEN** the system stores the relationship and marks it as active
- **AND** the system syncs `TenantUser.PrimaryReportsToUserId` for the subordinate

#### Scenario: Create project relationship
- **WHEN** a user creates a relationship with type "Project"
- **THEN** the system stores the relationship without affecting the primary superior sync

#### Scenario: Prevent duplicate relationship
- **WHEN** a user attempts to create a relationship that already exists for the same type and tenant
- **THEN** the system rejects the creation with a validation error

### Requirement: Validate cycle prevention
The system SHALL prevent creating relationships that would form a cycle in the hierarchy.

#### Scenario: Detect direct cycle
- **WHEN** a user attempts to create a relationship where the target superior is the same user
- **THEN** the system rejects with "cannot be own superior" error

#### Scenario: Detect indirect cycle
- **WHEN** a user attempts to create a relationship where the target superior is already a subordinate (direct or indirect)
- **THEN** the system rejects with "cycle detected" error

### Requirement: Update relationship priority
The system SHALL allow updating the priority of an existing relationship.

#### Scenario: Change priority
- **WHEN** a user updates the priority of a relationship
- **THEN** the system updates the priority value
- **AND** if the relationship is Functional, syncs `PrimaryReportsToUserId` if priority changes to/from 1

### Requirement: Delete relationship
The system SHALL allow soft-deleting an existing relationship.

#### Scenario: Soft delete
- **WHEN** a user deletes a relationship
- **THEN** the system sets `IsActive = false`
- **AND** the system logs the deletion in `RelationshipAuditLog`

### Requirement: Get user relationships
The system SHALL return all active reporting relationships for a user within a tenant.

#### Scenario: Get all relationships
- **WHEN** a user queries relationships for a specific user and tenant
- **THEN** the system returns all active relationships with type, priority, and related user info

#### Scenario: Get relationships by type
- **WHEN** a user queries relationships filtered by type
- **THEN** the system returns only relationships matching that type

### Requirement: Enforce maximum superiors limit
The system SHALL enforce a configurable maximum number of superiors per user.

#### Scenario: Exceed limit
- **WHEN** a user attempts to create a relationship that would exceed the maximum superiors limit
- **THEN** the system rejects with "maximum superiors exceeded" error

### Requirement: Audit relationship changes
The system SHALL log all relationship changes to an audit log.

#### Scenario: Log creation
- **WHEN** a relationship is created
- **THEN** the system creates an audit log entry with action "Created"

#### Scenario: Log deletion
- **WHEN** a relationship is deleted
- **THEN** the system creates an audit log entry with action "Deleted"
