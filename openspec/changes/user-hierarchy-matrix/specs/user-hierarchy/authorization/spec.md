## Purpose

Implementa el sistema de autorización basado en jerarquía con claims, policies y verificación en tiempo de ejecución.

## ADDED Requirements

### Requirement: Add hierarchy claims on authentication
The system SHALL include hierarchy-related claims when building the user principal.

#### Scenario: Include hierarchy level
- **WHEN** a user authenticates
- **THEN** the system adds a "hierarchy_level" claim with the user's role level

#### Scenario: Include direct superior IDs
- **WHEN** a user authenticates
- **THEN** the system adds a "direct_superior_ids" claim with JSON array of superior user IDs

#### Scenario: Include primary superior ID
- **WHEN** a user authenticates
- **THEN** the system adds a "primary_superior_id" claim with the Functional priority 1 superior ID

#### Scenario: Include relationship types
- **WHEN** a user authenticates
- **THEN** the system adds a "relationship_types" claim with JSON array of relationship types

### Requirement: Authorization policy CanManageHierarchy
The system SHALL enforce hierarchy management permissions based on role level.

#### Scenario: Manager can manage
- **WHEN** a user with level >= 70 attempts to access hierarchy management endpoints
- **THEN** the system grants access

#### Scenario: Non-manager denied
- **WHEN** a user with level < 70 attempts to access hierarchy management endpoints
- **THEN** the system denies access

### Requirement: Authorization policy CanViewSubordinates
The system SHALL enforce subordinate viewing permissions based on role level.

#### Scenario: Operator can view
- **WHEN** a user with level >= 40 attempts to view subordinates
- **THEN** the system grants access

#### Scenario: Worker denied
- **WHEN** a user with level < 40 attempts to view subordinates
- **THEN** the system denies access

### Requirement: Authorization policy CanLeadProject
The system SHALL enforce project leadership permissions based on role level.

#### Scenario: Coordinator can lead
- **WHEN** a user with level >= 60 attempts to lead a project
- **THEN** the system grants access

#### Scenario: Operator denied
- **WHEN** a user with level < 60 attempts to lead a project
- **THEN** the system denies access

### Requirement: Runtime authority verification
The system SHALL verify hierarchy authority at runtime for sensitive operations.

#### Scenario: Verify command authority
- **WHEN** a controller action requires hierarchy authorization
- **THEN** the system uses `HierarchyAuthorizationHandler` to check if the current user can command the target user

#### Scenario: Type-specific authority
- **WHEN** authority check specifies a relationship type
- **THEN** the system only considers relationships of that type

### Requirement: Hierarchy authorization handler
The system SHALL provide an authorization handler for hierarchy-based access control.

#### Scenario: Success when can command
- **WHEN** the current user can command the target user
- **THEN** the authorization handler succeeds

#### Scenario: Failure when cannot command
- **WHEN** the current user cannot command the target user
- **THEN** the authorization handler fails
