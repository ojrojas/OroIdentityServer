## Purpose

Return users that belong to a given catalogue role, with tenant scoping controlled by the caller's role. Administrator callers may query any role across all tenants; non-Administrator callers may only query non-Administrator roles within their own tenant.

## ADDED Requirements

### Requirement: Administrator can query any role across all tenants

The Administrator role SHALL be able to query users for any catalogue role (including Administrator) across all tenants. The `tenantId` parameter is optional for Administrator callers — when omitted, results span all tenants.

#### Scenario: Administrator queries Administrator role without tenant filter

- **WHEN** a caller whose catalogue role is `Administrator` requests users for the `Administrator` role without providing `tenantId`
- **THEN** the system returns every user across all tenants that is assigned the `Administrator` role

#### Scenario: Administrator queries Administrator role with a specific tenant

- **WHEN** a caller whose catalogue role is `Administrator` requests users for the `Administrator` role with a specific `tenantId`
- **THEN** the system returns every user in that tenant assigned the `Administrator` role

#### Scenario: Administrator queries a non-Administrator role

- **WHEN** a caller whose catalogue role is `Administrator` requests users for a role that is NOT `Administrator`
- **THEN** the system returns every user (across all tenants or in the specified tenant) assigned that role

### Requirement: Non-Administrator roles cannot query Administrator users

A caller whose catalogue role is NOT `Administrator` SHALL receive `403 Forbidden` when requesting users for the `Administrator` role, regardless of whether `tenantId` is provided.

#### Scenario: Non-Administrator queries Administrator role

- **WHEN** a caller whose catalogue role is NOT `Administrator` requests users for the `Administrator` role
- **THEN** the system returns `403 Forbidden` and does not return any users

### Requirement: Non-Administrator roles must provide tenantId

A caller whose catalogue role is NOT `Administrator` SHALL provide the `tenantId` query parameter. If `tenantId` is omitted, the system returns `400 Bad Request`.

#### Scenario: Non-Administrator omits tenantId

- **WHEN** a caller whose catalogue role is NOT `Administrator` requests users for a non-Administrator role without providing `tenantId`
- **THEN** the system returns `400 Bad Request`

### Requirement: Non-Administrator roles are scoped to their own tenant

A caller whose catalogue role is NOT `Administrator` SHALL only see users within their own home tenant. If the provided `tenantId` does not match the caller's home tenant, the system returns `403 Forbidden`.

#### Scenario: Non-Administrator queries their own tenant

- **WHEN** a caller whose catalogue role is NOT `Administrator` requests users for a non-Administrator role with their own `tenantId`
- **THEN** the system returns users in that tenant assigned the specified role

#### Scenario: Non-Administrator queries a different tenant

- **WHEN** a caller whose catalogue role is NOT `Administrator` requests users for a non-Administrator role with a `tenantId` that does not match their home tenant
- **THEN** the system returns `403 Forbidden`

### Requirement: Missing role returns empty result

When the specified role does not exist, the system SHALL return an empty list (not an error), consistent with the existing `GetUsersQuery` behavior.

#### Scenario: Role not found

- **WHEN** the specified role identifier does not correspond to any catalogue role
- **THEN** the system returns an empty list with `200 OK`

#### Scenario: No users match

- **WHEN** the specified role and tenant exist but no users match
- **THEN** the system returns an empty list with `200 OK`
