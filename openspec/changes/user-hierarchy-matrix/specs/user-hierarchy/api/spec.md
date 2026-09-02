## Purpose

Define los endpoints REST para la gestión de jerarquía de usuarios.

## ADDED Requirements

### Requirement: POST /api/hierarchy/relationships
The system SHALL provide an endpoint to create reporting relationships.

#### Scenario: Create relationship successfully
- **WHEN** a POST request is made with valid UserId, ReportsToUserId, Type, and Priority
- **THEN** the system returns 201 with the created relationship

#### Scenario: Validation error
- **WHEN** a POST request is made with invalid data (cycle, duplicate, etc.)
- **THEN** the system returns 400 with validation error details

### Requirement: PUT /api/hierarchy/relationships/{id}/priority
The system SHALL provide an endpoint to update relationship priority.

#### Scenario: Update priority successfully
- **WHEN** a PUT request is made with a valid relationship ID and new priority
- **THEN** the system returns 200 with updated relationship

### Requirement: DELETE /api/hierarchy/relationships/{id}
The system SHALL provide an endpoint to delete relationships.

#### Scenario: Delete relationship successfully
- **WHEN** a DELETE request is made with a valid relationship ID
- **THEN** the system returns 204 (soft delete)

### Requirement: GET /api/hierarchy/relationships/{userId}
The system SHALL provide an endpoint to get all relationships for a user.

#### Scenario: Get relationships
- **WHEN** a GET request is made for a user ID
- **THEN** the system returns 200 with all active relationships

### Requirement: GET /api/hierarchy/superiors/{userId?}
The system SHALL provide an endpoint to get direct superiors.

#### Scenario: Get superiors
- **WHEN** a GET request is made (with optional user ID)
- **THEN** the system returns 200 with direct superiors

### Requirement: GET /api/hierarchy/superiors/{userId?}/primary
The system SHALL provide an endpoint to get the primary superior.

#### Scenario: Get primary superior
- **WHEN** a GET request is made for primary superior
- **THEN** the system returns 200 with primary superior or null

### Requirement: GET /api/hierarchy/superiors/{userId?}/by-type/{type}
The system SHALL provide an endpoint to get superiors by relationship type.

#### Scenario: Filter by type
- **WHEN** a GET request is made with a specific relationship type
- **THEN** the system returns 200 with superiors of that type only

### Requirement: GET /api/hierarchy/subordinates/{userId?}
The system SHALL provide an endpoint to get direct subordinates.

#### Scenario: Get subordinates
- **WHEN** a GET request is made (with optional user ID)
- **THEN** the system returns 200 with direct subordinates

### Requirement: GET /api/hierarchy/subordinates/{userId?}/all
The system SHALL provide an endpoint to get all subordinates recursively.

#### Scenario: Get all subordinates
- **WHEN** a GET request is made for all subordinates
- **THEN** the system returns 200 with recursive subordinate tree

### Requirement: GET /api/hierarchy/chain/{userId?}
The system SHALL provide an endpoint to get the command chain.

#### Scenario: Get command chain
- **WHEN** a GET request is made for command chain
- **THEN** the system returns 200 with chain of command

### Requirement: GET /api/hierarchy/tree
The system SHALL provide an endpoint to get the organization tree.

#### Scenario: Get primary tree
- **WHEN** a GET request is made for organization tree
- **THEN** the system returns 200 with tree using primary relationships only

### Requirement: GET /api/hierarchy/tree/full
The system SHALL provide an endpoint to get the full organization tree.

#### Scenario: Get full tree
- **WHEN** a GET request is made for full tree
- **THEN** the system returns 200 with tree showing all relationships

### Requirement: GET /api/hierarchy/can-command/{commanderId}/{targetId}
The system SHALL provide an endpoint to check command authority.

#### Scenario: Check authority
- **WHEN** a GET request is made with commander and target IDs
- **THEN** the system returns 200 with boolean indicating authority

### Requirement: GET /api/hierarchy/level/{userId?}
The system SHALL provide an endpoint to get hierarchy level.

#### Scenario: Get level
- **WHEN** a GET request is made for hierarchy level
- **THEN** the system returns 200 with the user's hierarchy level

### Requirement: POST /api/hierarchy/sync-primary/{userId}
The system SHALL provide an endpoint to sync primary reports.

#### Scenario: Sync primary
- **WHEN** a POST request is made to sync primary
- **THEN** the system returns 200 and updates TenantUser.PrimaryReportsToUserId
