## Context

See `proposal.md` — Why. The permission catalogue is managed through `/api/permissions` (AdminOnly) and `IAdminPermissionService` but has no console surface. Permissions are granted to roles via `RolePermission` and reach tokens through the single resolution point `PermissionRepository.GetPermissionNamesByUserIdAsync` (used by the admin cookie, `AuthorizationController.Authorize`/`Accept`/`Exchange` and `userinfo`). Users have no direct grants.

The console patterns to mirror are the Identification Types pages (`IdentificationTypes.razor`, `IdentificationTypeCreate.razor`, `IdentificationTypeDetail.razor`) and the role↔permission assignment already added to `RoleDetail.razor`.

## Goals / Non-Goals

**Goals:**
- Make the permission catalogue reachable and manageable from the menu and a dedicated page.
- Surface permissions on the dashboard.
- Allow granting permissions directly to a user, combined with role-derived permissions, without a schema-breaking change.
- Keep a single resolution point so every token/cookie path picks up direct permissions automatically.

**Non-Goals:**
- Per-tenant permissions. `Permission` and `UserPermission` are global catalogue records.
- Changing the claim contract (`Claim("permission", <name>)`) or destinations.
- Wildcard matching.

## Decisions

### D1: Direct permissions are additive to role permissions

A user's effective permission names are `distinct(role permissions ∪ direct user permissions)`. `GetPermissionNamesByUserIdAsync` unions both sources; deactivated roles are excluded (the `Role` query filter), and a deactivated user is irrelevant (they cannot authenticate).

- **Why**: The existing token emission depends on this one method, so a union here is the smallest, safest change and guarantees consistency across cookie, access token, id token and `userinfo`.
- **Alternative rejected**: a separate resolution path per token type — duplicated logic and drift.

### D2: `UserPermission` mirrors `UserRole`

`UserPermission(UserId, PermissionId)` with a composite key, configured exactly like `UserRole`/`RolePermission` (value-object conversions). The `User` aggregate owns an `_permissions` collection with `AddPermission`/`RemovePermission`; assignment is a replace-set command mirroring `AssignRolesToUserCommand`.

- **Why**: Consistency with the existing user↔role model; reuses the proven detached/tracking handling in `Repository.UpdateAsync` when the user is loaded with tracking (see the earlier `NoTracking` fix for roles).
- **Note**: `UserRepository.GetUserByIdAsync` must load the collection with tracking for updates; the query path uses the specification include.

### D3: Catalogue page is full CRUD, system permissions protected

`Permissions.razor` lists the catalogue; `PermissionCreate.razor`/`PermissionDetail.razor` create/edit. `IsSystem` permissions render read-only and hide the delete action (the aggregate also rejects modification).

### D4: Dashboard card is gated to Admin/Administrator

The permissions endpoint is `AdminOnly`, so the card and the recent list only load for Admin/Administrator. `Dashboard.razor`/`MainLayout.razor` currently name that check `isMasterAdmin`; it is renamed to `isAdmin` for clarity (it is `IsInRole("Admin") || IsInRole("Administrator")`).

### D5: UserDetail shows direct and effective permissions

`UserDetail.razor` gets a direct-permission checklist (like the role picker) and a read-only effective-permissions panel computed as `union(role-derived, direct)` so operators can see the full picture.
