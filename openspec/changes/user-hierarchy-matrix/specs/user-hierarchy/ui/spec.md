## Purpose

Define los componentes Blazor para visualización y gestión del organigrama jerárquico.

## ADDED Requirements

### Requirement: Organization tree component
The system SHALL provide a Blazor component to display the organizational hierarchy as a tree.

#### Scenario: Display tree
- **WHEN** the OrganizationTree component is rendered with a tenant ID
- **THEN** the component displays the hierarchical tree with user names, roles, and hierarchy levels

#### Scenario: Select user
- **WHEN** a user clicks on a node in the tree
- **THEN** the component fires an OnUserSelected event with the selected user data

#### Scenario: Display secondary relationships
- **WHEN** viewing the full tree
- **THEN** nodes with multiple relationships show secondary relationship badges

### Requirement: Relationship manager component
The system SHALL provide a Blazor component for managing reporting relationships.

#### Scenario: Create relationship
- **WHEN** a manager uses the relationship manager
- **THEN** the component allows selecting a user and target superior with type and priority

#### Scenario: Edit relationship priority
- **WHEN** a manager selects an existing relationship
- **THEN** the component allows updating the priority

#### Scenario: Delete relationship
- **WHEN** a manager selects a relationship and clicks delete
- **THEN** the component confirms and soft-deletes the relationship

### Requirement: Hierarchy management page
The system SHALL provide a page for managing the organization hierarchy.

#### Scenario: View page
- **WHEN** a user navigates to /hierarchy/manage
- **THEN** the system displays the hierarchy management page with tree and details panel

#### Scenario: User details panel
- **WHEN** a user is selected in the tree
- **THEN** the details panel shows user info, relationships, and allows reassignment

### Requirement: Filter by relationship type
The system SHALL allow filtering the hierarchy view by relationship type.

#### Scenario: Filter view
- **WHEN** a user selects a relationship type filter
- **THEN** the tree updates to show only relationships of that type

### Requirement: Filter by hierarchy level
The system SHALL allow filtering the hierarchy view by hierarchy level.

#### Scenario: Filter by level
- **WHEN** a user selects a hierarchy level filter
- **THEN** the tree updates to show only users at or above that level
