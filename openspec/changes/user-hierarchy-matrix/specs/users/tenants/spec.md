## Purpose

Define la entidad TenantUser con soporte para jerarquía de usuarios.

## MODIFIED Requirements

### Requirement: TenantUser has primary reports to
The system SHALL support a primary superior reference on TenantUser.

#### Scenario: Primary reports to set
- **WHEN** a TenantUser has a Functional relationship with priority 1
- **THEN** the system syncs PrimaryReportsToUserId with that relationship

#### Scenario: Primary reports to null
- **WHEN** a TenantUser has no Functional priority 1 relationship
- **THEN** PrimaryReportsToUserId is null

### Requirement: TenantUser has hierarchy level
The system SHALL support a hierarchy level property on TenantUser.

#### Scenario: Level from role
- **WHEN** a TenantUser is created or updated
- **THEN** the system calculates HierarchyLevel from the user's role in that tenant

#### Scenario: Level update
- **WHEN** a user's role changes in a tenant
- **THEN** the system updates HierarchyLevel accordingly
