## Context

See `proposal.md` — Why. The project models user authorization permissions as a first-class domain aggregate (`src/Core/Modules/Permissions/Aggregates/Permission.cs`, `Name = "{Provider}.{Resource}.{Action}".ToLowerInvariant()`), linked to roles through `RolePermission` (`Role.AddPermission`/`RemovePermission`). The IdP already issues tokens through OpenIddict (`AuthorizationController`) and an admin cookie (`AdminPasswordSignInService`), and both currently emit only `ClaimTypes.Role` (plus `tenant_id`, hierarchy claims, and `is_master_admin`).

Two unrelated concepts share the name "Permissions":

1. **Client permissions** — OpenIddict application permissions (`ept:`, `gt:`, `rst:`, `scp:`, `ft:`), stored on `OpenIddictApplicationDescriptor.Permissions`, edited in `ApplicationDetail.razor` via `OpenIddictPermissionOptions.cs`. They describe what an OAuth client may do.
2. **User permissions** — domain `Permission.Name`, assigned to roles, describing what a signed-in user is authorized to do. This is the subject of this change.

## Goals / Non-Goals

**Goals:**
- Give user permissions the same mechanics as IdentityModel claims: a claim type plus a claim value, usable with `RequireClaim`, `HasClaim`, `User.IsInRole`-style checks.
- Make them reach the places authorization decisions happen: the admin cookie, access tokens, and (on request) the id token and userinfo.
- Make them enforceable with exact-match policies and manageable from the admin console.
- Eliminate the client-vs-user permission ambiguity in code, docs, and the new skill.

**Non-Goals:**
- Wildcard/pattern matching of permission names. Matching is exact. `system.*.*` is a literal permission name.
- Per-tenant permissions. `Permission` and `Role` are global catalogue entities; tenant scoping is out of scope.
- Replacing role/hierarchy authorization. Permissions are additive.
- Changing the OpenIddict client-permission model.

## Decisions

### D1: One claim per permission, fixed type `permission`

Emit `new Claim("permission", "oropos.sales.read")` for every granted permission, added with `identity.SetClaims(AuthorizationClaimTypes.Permission, names)`.

- **Why**: This is the exact IdentityModel behavior (type + value, multiple values under one type) and works out of the box with `RequireClaim("permission", "x")`, `HasClaim`, and standard JSON serialization. Reusing IdentityModel's own claim type *names* is explicitly avoided; the *mechanism* is mirrored.
- **Alternatives rejected**:
  - `Claim("oropos.sales.read", "true")` — literal "permission is the type", but creates an unbounded number of claim types and forces `RequireClaim("oropos.sales.read", "true")`; hostile to token size and to dynamic policies.
  - `Claim("oropos.sales", "read")` — preserves the aggregate's two dimensions but splits one permission across a type/value pair that does not map 1:1 to `Permission.Name`, complicating assignment and policy naming.
  - Namespaced `oro:permission` — avoids collision with OAuth scopes but is non-standard; `permission` is already unambiguous inside a claims identity.

### D2: New `permissions` scope, destinations mirror `roles`

- Access token: always.
- Identity token and `userinfo`: only when the `permissions` scope is granted.
- `GetDestination.GetDestinations` gets a `case AuthorizationClaimTypes.Permission` that returns `Destinations.AccessToken` and, if `principal.HasScope(AuthorizationScopes.Permissions)`, `Destinations.IdentityToken`.
- `AuthorizationController.Userinfo` adds `permission` only when `User.HasScope(AuthorizationScopes.Permissions)`.

- **Why**: Resource servers enforce from the access token; leaking authorization data into the id token / userinfo by default is unnecessary. This is exactly how `roles` behaves today (`Claims.Role` → access always, id token with `roles` scope), so it is the least surprising choice for existing integrators.
- **Alternative rejected**: permissions only in the access token — makes SPA UI gating require decoding the access token or an extra userinfo/claims round-trip with no scope signal.

### D3: Exact-match enforcement via `perm:<name>` policies

`PermissionAuthorizationHandler` succeeds when `context.User.HasClaim(AuthorizationClaimTypes.Permission, requirement.Permission)`. A `PermissionPolicyProvider` derives a policy on demand from a policy name of the form `perm:<permission-name>`, so callers write `[Authorize(Policy = "perm:oropos.sales.read")]` without registering each permission.

- **Why**: Permissions are data (created/edited at runtime via `/api/permissions`), so policies cannot be enumerated at startup. A dynamic provider keeps the call sites declarative and the handler trivial.
- **Alternative rejected**: pre-registering policies for every permission in the catalogue — requires a DB round-trip at startup and breaks when permissions are added later.

### D4: Resolve permission names with a dedicated query, not by extending role DTOs

Add `IPermissionRepository.GetPermissionNamesByUserIdAsync(UserId)` returning distinct `Permission.Name` for the user's active roles, and a `GetPermissionNamesByUserIdQuery`/handler wrapping it. Also fix `RoleRepository.GetRolesByUserIdAsync` to include `RolePermissions` (a latent bug: `RoleDto.Claims` is always empty today).

- **Why**: Token issuance needs names, not ids; `RolePermissionDto` carries only ids. A dedicated query keeps the token path cheap (one query, distinct names) and independent of the role DTO shape.
- **Note**: The `Role` query filter is `IsActive`, so deactivated roles are automatically excluded.

### D5: Role↔permission management as a replace-set command

`SetRolePermissionsCommand(RoleId, IReadOnlyCollection<Guid> PermissionIds)` loads the role with `GetRoleWithPermissionsSpecification`, diffs against the requested set, and calls `Role.AddPermission`/`Role.RemovePermission`. Exposed as `PUT /api/roles/{id}/permissions` (AdminOnly) and surfaced in `RoleDetail.razor` with a checkbox list built from `/api/permissions`.

- **Why**: A replace-set endpoint is idempotent and matches how the UI works (a checklist with a single Save). Reuses the aggregate's existing invariants (duplicate/not-found guards) instead of mutating the join table directly.
- **Alternative rejected**: add/remove-per-permission endpoints — chattier, and the UI would need to reconcile state.

### D6: Keep `system.*.*` literal

No wildcard matching. The seeded `system.*.*` permission is an ordinary name; operators decide whether to assign it. Documented explicitly.

- **Why**: The user chose exact matching; introducing a wildcard exception would create two mental models and complicate the handler.
