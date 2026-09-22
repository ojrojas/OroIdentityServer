## Purpose

Surface the domain permission catalogue on the admin dashboard.

## ADDED Requirements

### Requirement: Permission count card

The dashboard SHALL show a card with the total number of domain permissions, visible to administrators.

#### Scenario: Card shows the total

- **WHEN** an administrator opens the dashboard
- **THEN** a permissions card shows the total number of permissions in the catalogue

#### Scenario: Card hidden for non-administrators

- **WHEN** a user without the Admin or Administrator role opens the dashboard
- **THEN** the permissions card is not rendered

#### Scenario: Card navigates to the catalogue

- **WHEN** an administrator activates the permissions card
- **THEN** the console navigates to the permissions page

### Requirement: Recently created permissions

The dashboard "recently created" list SHALL be able to include permissions, linking each entry to its detail page.

#### Scenario: Permission appears in recent list

- **WHEN** a permission was created recently and the dashboard builds its recent list
- **THEN** the permission is a candidate entry labelled with the permissions label and linked to `/permissions/{id}`
