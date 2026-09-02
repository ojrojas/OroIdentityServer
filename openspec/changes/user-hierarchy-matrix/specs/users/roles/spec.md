## Purpose

Define el aggregate Role con soporte para niveles jerárquicos.

## MODIFIED Requirements

### Requirement: Role has hierarchy level
The system SHALL support a hierarchy level property on Role to define authority level.

#### Scenario: Role with level
- **WHEN** a Role is created or updated
- **THEN** the system stores a Level property (int, 10-100)
- **AND** higher values indicate more authority

#### Scenario: Default level
- **WHEN** a Role is created without specifying a level
- **THEN** the system defaults to level 10

### Requirement: Role has parent role
The system SHALL support optional parent role reference for role hierarchy.

#### Scenario: Role with parent
- **WHEN** a Role is created with a ParentRoleId
- **THEN** the system establishes the role hierarchy relationship

#### Scenario: Role without parent
- **WHEN** a Role is created without ParentRoleId
- **THEN** the system treats it as a root role in the role hierarchy
