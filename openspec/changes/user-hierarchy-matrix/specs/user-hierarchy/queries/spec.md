## Purpose

Proporciona consultas jerárquicas recursivas para navegar la cadena de mando, obtener subordinados, superiores y árboles organizacionales.

## ADDED Requirements

### Requirement: Get direct superiors
The system SHALL return all direct superiors of a user within a tenant.

#### Scenario: User with multiple superiors
- **WHEN** a user queries direct superiors
- **THEN** the system returns all users who have an active reporting relationship with the target user
- **AND** each result includes the relationship type and priority

#### Scenario: User with no superiors
- **WHEN** a user with no superiors queries direct superiors
- **THEN** the system returns an empty collection

### Requirement: Get primary superior
The system SHALL return the primary superior of a user (Functional relationship with priority 1).

#### Scenario: User has primary superior
- **WHEN** a user queries primary superior and has a Functional relationship with priority 1
- **THEN** the system returns that superior

#### Scenario: User has no primary superior
- **WHEN** a user queries primary superior and has no Functional priority 1 relationship
- **THEN** the system returns null

### Requirement: Get direct subordinates
The system SHALL return all direct subordinates of a user within a tenant.

#### Scenario: Manager with subordinates
- **WHEN** a manager queries direct subordinates
- **THEN** the system returns all users who report directly to that manager
- **AND** results include hierarchy level and role name

#### Scenario: Filter by relationship type
- **WHEN** a manager queries subordinates filtered by relationship type
- **THEN** the system returns only subordinates with that relationship type

### Requirement: Get all subordinates recursively
The system SHALL return all subordinates (direct and indirect) using the primary hierarchy path.

#### Scenario: Recursive traversal
- **WHEN** a user queries all subordinates
- **THEN** the system uses recursive CTEs to traverse the hierarchy
- **AND** returns all users in the subordinate tree

#### Scenario: Cycle prevention
- **WHEN** traversing the hierarchy
- **THEN** the system uses a visited array to prevent infinite loops

#### Scenario: Depth limit
- **WHEN** traversing the hierarchy
- **THEN** the system respects the maximum depth configuration (default 10)

### Requirement: Get command chain
The system SHALL return the chain of command from a user up to the root of the hierarchy.

#### Scenario: Full chain
- **WHEN** a user queries the command chain
- **THEN** the system returns all superiors up to the root, following the primary hierarchy path

#### Scenario: User at root
- **WHEN** a user at the root of the hierarchy queries the command chain
- **THEN** the system returns an empty collection

### Requirement: Check command authority
The system SHALL verify if one user can command another based on hierarchy relationships.

#### Scenario: User can command direct subordinate
- **WHEN** a user checks if they can command a direct subordinate
- **THEN** the system returns true

#### Scenario: User can command indirect subordinate
- **WHEN** a user checks if they can command an indirect subordinate
- **THEN** the system returns true (following primary hierarchy)

#### Scenario: User cannot command unrelated user
- **WHEN** a user checks if they can command a user with no hierarchy relationship
- **THEN** the system returns false

#### Scenario: Multiple superiors authority
- **WHEN** a user has multiple superiors
- **THEN** each superior has full authority over that user

### Requirement: Get organization tree
The system SHALL return the complete organizational tree for a tenant.

#### Scenario: Primary hierarchy tree
- **WHEN** a user queries the organization tree
- **THEN** the system returns a tree structure using only Functional primary relationships

#### Scenario: Full hierarchy tree
- **WHEN** a user queries the full organization tree
- **THEN** the system returns a tree showing all relationships with secondary relationships listed per node

### Requirement: Get hierarchy level
The system SHALL return the effective hierarchy level of a user within a tenant.

#### Scenario: Level from role
- **WHEN** a user queries hierarchy level
- **THEN** the system returns the level derived from the user's role in that tenant
