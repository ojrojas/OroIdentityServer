## Purpose

Resolve a signed-in user's domain authorization permissions from their active roles and emit them as `permission` claims, with the same mechanics as IdentityModel claims (a fixed claim type plus one value per granted permission).

## ADDED Requirements

### Requirement: Permission names resolved from active roles

The system SHALL resolve the distinct `Permission.Name` values granted to a user through the active roles assigned to that user, and SHALL ignore deactivated roles.

#### Scenario: User with permissions through active roles

- **WHEN** a user has active roles that are each assigned one or more domain permissions
- **THEN** the resolved set contains the distinct permission names of all those roles

#### Scenario: Deactivated role is ignored

- **WHEN** one of the user's roles is deactivated
- **THEN** the permission names contributed by that role are not resolved

#### Scenario: No roles or no permissions

- **WHEN** the user has no active roles or their roles have no permissions
- **THEN** the resolved set is empty

### Requirement: Permission claims emitted with the `permission` claim type

The system SHALL emit one `permission` claim per granted permission, whose value is the permission name (`Provider.Resource.Action`), in the admin authentication cookie and in the OIDC identity used to issue tokens.

#### Scenario: Admin cookie carries permission claims

- **WHEN** an administrator signs in through the admin sign-in service
- **THEN** the issued cookie principal contains one `permission` claim for each permission of the user's active roles

#### Scenario: Authorization endpoint carries permission claims

- **WHEN** the authorization endpoint completes an authorization for a user with permissions
- **THEN** the claims identity used by OpenIddict contains one `permission` claim per granted permission

#### Scenario: Token exchange carries permission claims

- **WHEN** an authorization code or refresh token is exchanged at the token endpoint
- **THEN** the refreshed identity re-emits the user's current `permission` claims

### Requirement: Permission claim destinations

The system SHALL include `permission` claims in the access token always, and in the identity token and `userinfo` response only when the `permissions` scope has been granted.

#### Scenario: Access token always includes permissions

- **WHEN** an access token is issued for a user with permissions regardless of the requested scopes
- **THEN** the access token contains the `permission` claims

#### Scenario: Identity token includes permissions only with the scope

- **WHEN** the `permissions` scope is granted for the request
- **THEN** the identity token also contains the `permission` claims

#### Scenario: Identity token omits permissions without the scope

- **WHEN** the `permissions` scope is not granted for the request
- **THEN** the identity token does not contain `permission` claims

#### Scenario: Userinfo includes permissions only with the scope

- **WHEN** `userinfo` is called with an access token that has the `permissions` scope
- **THEN** the response contains a `permission` entry listing the user's permission names

#### Scenario: Userinfo omits permissions without the scope

- **WHEN** `userinfo` is called with an access token that lacks the `permissions` scope
- **THEN** the response does not contain a `permission` entry

### Requirement: Permissions scope is registered and grantable

The system SHALL register a `permissions` scope and SHALL allow client applications to be granted `scp:permissions` through the admin application configuration.

#### Scenario: Scope advertised by discovery

- **WHEN** a client reads the OpenID Connect discovery document
- **THEN** `permissions` is listed among the supported scopes

#### Scenario: Client granted the permissions scope

- **WHEN** an application is configured with the `scp:permissions` client permission and the client requests the `permissions` scope
- **THEN** the scope is granted for the resulting authorization
