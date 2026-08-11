## Purpose

The admin dashboard shows live, per-entity counts of records created today so operators can see activity at a glance without inspecting each module.

## ADDED Requirements

### Requirement: Per-entity created-today deltas

The dashboard SHALL display, for each supported entity type, a count of records created during the current calendar day (server-local UTC day), derived from real data in the repository. Supported entity types: users, roles, tenants, connected users, applications, scopes, and identification types. Each stat card SHALL show the entity's own name and its own delta; the user stat card SHALL NOT reuse another entity's count.

#### Scenario: Users stat shows only users created today

- **WHEN** the dashboard loads and 3 users, 2 roles, and 2 tenants were created today
- **THEN** the Users card shows the number 3, the Roles card shows 2, and the Tenants card shows 2

#### Scenario: No records created today

- **WHEN** no records of a given entity type were created today
- **THEN** that entity's delta is 0

#### Scenario: Connected users reflects live sessions

- **WHEN** the dashboard loads
- **THEN** the connected-users card shows the count of distinct users with active sessions, not records created today

#### Scenario: Tenant-scoped entities respect the selected tenant

- **WHEN** a tenant is selected in the tenant switcher and the dashboard loads
- **THEN** deltas for tenant-scoped entities (users, sessions) reflect only records belonging to the selected tenant

### Requirement: Dynamic recently created list

The dashboard "recently created" section SHALL include the latest records created today across all supported entity types (users, roles, tenants, applications, scopes, identification types, and others as supported), each labeled with its entity type, and SHALL fall back to the most recent records when nothing was created today.

#### Scenario: Recently created mixes entity types

- **WHEN** users, roles, and tenants were created today
- **THEN** the recently created list shows entries from all three entity types, each with the correct entity name and type label

#### Scenario: Nothing created today

- **WHEN** no records were created today
- **THEN** the recently created list shows the most recent records instead of being empty

### Requirement: Dashboard deltas are not hardcoded

The dashboard SHALL derive every "created today" delta from the data store at load time; no entity's delta SHALL be a hardcoded constant or a copy of another entity's count.

#### Scenario: Delta reflects database state

- **WHEN** a user is created and the dashboard reloads
- **THEN** the Users delta increases by one
