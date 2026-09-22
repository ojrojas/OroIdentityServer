## Why

The domain already models user authorization permissions as the `Permission` aggregate (`Provider.Resource.Action`, normalized to `Permission.Name`), and roles can hold them through `RolePermission` (`Role.AddPermission`/`RemovePermission`). In practice they are dead weight:

- `AuthorizationController` (`Authorize`, `Accept`, `Exchange`) and `AdminPasswordSignInService.BuildPrincipalAsync` emit only role claims; permission names never reach any token.
- `GetRolesByUserIdQuery` returns `RolePermissionDto` (role id + permission id only) and the controller ignores it; `RoleRepository.GetRolesByUserIdAsync` does not include `RolePermissions`, so the navigation is empty anyway.
- There is no shared claim type or OAuth scope for permissions, no authorization requirement/handler, and no API/UI to assign permissions to a role (only the seeder does it). `RoleDetail.razor` just shows a count.
- The word "Permissions" collides: OpenIddict **client permissions** (`ept:*`, `gt:*`, `rst:*`, `scp:*`, `ft:*`, stored on the application) vs **user permissions** (domain `Permission.Name`). This collision is the root cause of the recurring misinterpretation by developers and LLMs.

## What Changes

- **Contract**: add `AuthorizationClaimTypes.Permission = "permission"` and a new `permissions` scope (`AuthorizationScopes.Permissions`). A user permission is emitted as `new Claim("permission", Permission.Name)`, i.e. standard IdentityModel `Claim(type, value)` behavior (fixed type, one value per granted permission). Register the scope in OpenIddict, in the admin UI options (`scp:permissions`), and on the seeded admin client.
- **Load**: add `IPermissionRepository.GetPermissionNamesByUserIdAsync` plus `GetPermissionNamesByUserIdQuery`/handler that resolves the distinct permission names of a user's active roles (`UserRole → Role → RolePermission → Permission`), and fix the missing `RolePermissions` include in `RoleRepository.GetRolesByUserIdAsync`.
- **Emit**: add `permission` claims in the admin cookie (`AdminPasswordSignInService`) and in the OIDC identity built by `Authorize`/`Accept`/`Exchange`. Destinations: access token always; identity token and `userinfo` only when the `permissions` scope is granted (mirrors the existing `roles` behavior).
- **Enforce**: add `PermissionRequirement`, an exact-match `PermissionAuthorizationHandler`, and a `PermissionPolicyProvider` so `[Authorize(Policy = "perm:<name>")]` works without pre-registering every permission. Add a client-side `PermissionView` component for the Blazor console.
- **Manage**: add `SetRolePermissionsCommand` + handler, `PUT /api/roles/{id}/permissions` (AdminOnly), client service methods, and a permission checklist in `RoleDetail.razor` reusing `PermissionChecklist.razor`.
- **Document**: new `docs/authorization.md` (canonical glossary, contract, destinations, enforcement, integration examples, troubleshooting, explicit "client permission vs user permission" disambiguation) and README updates. New `oridentityserver-skills` skill (mirrored to `.agents`, `.opencode`, `.claude`) guiding agents that integrate authentication/authorization into other applications.
- **Matching is exact**: no wildcards. `system.*.*` is an ordinary literal permission; operators decide whether to assign it.

## Capabilities

### New Capabilities

- `authorization/permission-claims`: the IdP SHALL resolve a user's domain permissions from their active roles and emit them as `permission` claims in the admin cookie and in OIDC tokens, with the documented destinations.
- `authorization/permission-policies`: the IdP SHALL enforce exact-match permission policies (`perm:<permission-name>`) against the `permission` claim.
- `roles/permission-assignment`: the IdP SHALL expose an admin API and UI to replace the set of domain permissions assigned to a role.

### Modified Capabilities

- None. No existing specs cover this area.

## Impact

- **Shared**: `AuthorizationClaimTypes`, new `AuthorizationScopes`.
- **Application**: new `GetPermissionNamesByUserIdQuery`/handler, `SetRolePermissionsCommand`/handler.
- **Core/Infra**: `IPermissionRepository`/`PermissionRepository`, `RoleRepository.GetRolesByUserIdAsync` include fix.
- **IdentityServer**: `AuthorizationController`, `AdminPasswordSignInService`, `GetDestination`, `OpenIddictServerConfiguration`, `CookieAuthHandlerSetup`, new `Authorization/PermissionRequirement.cs`, `PermissionAuthorizationHandler.cs`, `PermissionPolicyProvider.cs`, `AdminRolesApiEndpoints`.
- **Client**: `IAdminRoleService`/`AdminRoleService`, `RoleDetail.razor`, new `Components/Shared/PermissionView.razor`.
- **Seeder**: `scp:permissions` on the seeded admin client.
- **Docs/Skill**: `docs/authorization.md`, `README.md`, `.agents/.opencode/.claude/skills/oridentityserver-skills`.
- **No database schema change** (permissions and role-permission tables already exist); no new NuGet packages.
- **Tests**: unit (permission query, exact-match handler) and integration (token contains `permission`; policy grants/denies; userinfo with `permissions` scope).
