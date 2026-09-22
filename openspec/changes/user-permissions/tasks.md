## 1. Contract (shared)

- [x] 1.1 Add `public const string Permission = "permission";` to `src/Shared/Authorization/AuthorizationClaimTypes.cs`.
- [x] 1.2 Create `src/Shared/Authorization/AuthorizationScopes.cs` with `public const string Permissions = "permissions";`.
- [x] 1.3 Register `AuthorizationScopes.Permissions` in `OpenIddictServerConfiguration.RegisterScopes`.
- [x] 1.4 Add `new("scp:permissions", "permissions")` to `OpenIddictPermissionOptions.Scopes`.
- [x] 1.5 Add `scp:permissions` to the seeded admin client in `DatabaseSeeder`.

## 2. Load (User → Role → Permission.Name)

- [x] 2.1 Add `GetPermissionNamesByUserIdAsync(UserId, CancellationToken)` to `IPermissionRepository`.
- [x] 2.2 Implement it in `PermissionRepository` (join `UserRoles → Roles → RolePermissions → Permissions`, distinct names, active roles only).
- [x] 2.3 Add `GetPermissionNamesByUserIdQuery` + handler under `Application/Modules/Permissions/Queries`.
- [x] 2.4 Fix `RoleRepository.GetRolesByUserIdAsync` to include `RolePermissions`.

## 3. Emit

- [x] 3.1 `AuthorizationController`: helper to add permission claims; call it in `Authorize`, `Accept`, `Exchange`.
- [x] 3.2 `AdminPasswordSignInService.BuildPrincipalAsync`: add permission claims to the admin cookie.
- [x] 3.3 `GetDestination`: `permission` → access token always; id token with `permissions` scope.
- [x] 3.4 `AuthorizationController.Userinfo`: return `permission` with `permissions` scope.

## 4. Enforce

- [x] 4.1 Create `Authorization/PermissionRequirement.cs`.
- [x] 4.2 Create `Authorization/PermissionAuthorizationHandler.cs` (exact match).
- [x] 4.3 Create `Authorization/PermissionPolicyProvider.cs` (`perm:<name>`).
- [x] 4.4 Register handler + policy provider in `CookieAuthHandlerSetup.AddAdminAuthorization`.
- [x] 4.5 Create `Components/Shared/PermissionView.razor` for client-side gating.

## 5. Manage role↔permission

- [x] 5.1 Create `SetRolePermissionsCommand` + handler.
- [x] 5.2 Add `PUT /api/roles/{id}/permissions` (AdminOnly) in `AdminRolesApiEndpoints`.
- [x] 5.3 Add `SetRolePermissionsAsync` to `IAdminRoleService` + `ServerAdminRoleService` + `AdminRoleService`.
- [x] 5.4 Add the permission checklist to `RoleDetail.razor` (reuse `PermissionChecklist.razor` / permission list).
- [x] 5.5 Load the role with tracking in `RoleRepository.GetWithPermissionsAsync` (the context is globally `NoTracking`, so an update path must opt back in, otherwise keyed children are marked `Modified` instead of `Added`).

## 6. Documentation

- [x] 6.1 Create `docs/authorization.md`.
- [x] 6.2 Update `README.md` (disambiguation, scopes/claims table, userinfo, policies, integration).

## 7. Skill

- [x] 7.1 Create `.agents/skills/oridentityserver-skills/SKILL.md`.
- [x] 7.2 Mirror to `.opencode/skills/oridentityserver-skills/SKILL.md` and `.claude/skills/oridentityserver-skills/SKILL.md`.

## 8. Tests

- [x] 8.1 Unit test: `PermissionAuthorizationHandler` exact match grants/denies; `PermissionPolicyProvider` builds policies.
- [x] 8.2 Unit test: permission-name resolution for a user's active roles.
- [x] 8.3 Unit test: `SetRolePermissionsCommandHandler` add/remove/idempotent/not-found/unknown-permission, including a NoTracking fresh-context persistence test.
- [x] 8.4 Integration test: `PUT /api/roles/{id}/permissions` round-trip, unknown permission rejected, Manager forbidden.
- [ ] 8.5 (Follow-up) Integration test asserting `permission` claims in the access token / `userinfo` with the `permissions` scope.

## 9. Verification

- [x] 9.1 `dotnet build OroIdentityServer.slnx` — succeeded.
- [x] 9.2 `dotnet test tests/Infraestructure.UnitTests` — 55 passed.
- [x] 9.3 `dotnet test tests/Server.Tests --filter RolePermissionsApiTests` — 3 passed. (6 pre-existing failures in unrelated tests reproduce on the base commit.)
