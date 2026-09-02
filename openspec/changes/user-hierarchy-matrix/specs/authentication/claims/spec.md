## Purpose

Define los claims de jerarquía incluidos en el proceso de autenticación.

## MODIFIED Requirements

### Requirement: Authentication includes hierarchy claims
The system SHALL include hierarchy-related claims in the user principal.

#### Scenario: Hierarchy level claim
- **WHEN** a user authenticates
- **THEN** the system adds a "hierarchy_level" claim with the user's effective level

#### Scenario: Direct superior IDs claim
- **WHEN** a user authenticates
- **THEN** the system adds a "direct_superior_ids" claim as JSON array

#### Scenario: Primary superior ID claim
- **WHEN** a user authenticates
- **THEN** the system adds a "primary_superior_id" claim

#### Scenario: Relationship types claim
- **WHEN** a user authenticates
- **THEN** the system adds a "relationship_types" claim as JSON array
