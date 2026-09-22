# Authorization in OroIdentityServer

This document is the canonical reference for how **user authorization permissions** work in
OroIdentityServer: how they are modeled, how they reach tokens, and how they are enforced.
It exists because two unrelated concepts in this codebase are both called "Permissions" and
mixing them up is the single most common integration mistake.

---

## 0. TL;DR for agents and integrators

- A **user permission** is a domain entity (`Permission`) whose name is
  `provider.resource.action` (lowercased), e.g. `oropos.sales.read`.
- It is granted to users **through roles** (`UserRole → Role → RolePermission → Permission`).
- It is emitted as a standard claim: **`Claim("permission", "oropos.sales.read")`** — one claim
  per granted permission. This mirrors IdentityModel `Claim(type, value)` behavior; it does
  **not** reuse IdentityModel claim type names.
- The **access token always** contains `permission` claims. The **id token** and **userinfo**
  contain them only when the client requests the **`permissions`** scope.
- Enforcement on the server uses exact-match policies named `perm:<permission-name>`:
  `[Authorize(Policy = "perm:oropos.sales.read")]`.
- **Do NOT confuse** these with OpenIddict **client permissions** (`ept:`, `gt:`, `rst:`,
  `scp:`, `ft:`), which describe what an OAuth **client** may do and live on the application,
  not on the user. See §1.

---

## 1. Two different things called "Permissions"

| | **Client permissions** | **User permissions** |
|---|---|---|
| What it is | Capability of the OAuth **client application** | Authorization grant of a **signed-in user** |
| Where it lives | `OpenIddictApplicationDescriptor.Permissions` (the app record) | `Permission` aggregate + `RolePermission` |
| Format | `ept:token`, `gt:authorization_code`, `rst:code`, `scp:profile`, `ft:pkce` | `provider.resource.action` (e.g. `oropos.sales.read`) |
| Managed in | `ApplicationDetail.razor` / `OpenIddictPermissionOptions.cs` | `RoleDetail.razor` / `PUT /api/roles/{id}/permissions` |
| Emitted as claim | No | Yes, `Claim("permission", <name>)` |
| Answers | "May this app call the token endpoint / request this scope?" | "May this user perform this action?" |

If you see `Permissions` on `OpenIddictApplicationModel`, `ApplicationDescriptor`, or in
`OpenIddictPermissionOptions`, it is the **client** kind. If you see `Permission` (singular) on
the domain side, or `GetPermissionNamesByUserId`, it is the **user** kind.

---

## 2. User permission model

- `Permission` (`src/Core/Modules/Permissions/Aggregates/Permission.cs`) has `Provider`,
  `Resource`, `Action`, `Scope`, `IsSystem`, and a derived `Name = "{Provider}.{Resource}.{Action}"`
  (lowercased, `src/Core/Modules/Permissions/Aggregates/Permission.cs:78`).
- Permissions are assigned to **roles** via `RolePermission` (`Role.AddPermission` /
  `Role.RemovePermission`). Users get permissions transitively through `UserRole`.
- Permissions can **also be granted directly to a user** via `UserPermission`
  (`User.AddPermission` / `User.RemovePermission`), independently of roles.
- `Role`, `Permission` and `UserPermission` are **global catalogue** entities (no tenant column).
  Tenant scoping is handled by roles/claims such as `tenant_id`, not by permissions.
- The seeded bootstrap permission is `system.*.*` (a **literal** name). Matching is exact, so
  assigning it grants nothing by itself unless a policy asks for exactly `perm:system.*.*`.
  Operators decide which permissions to assign.

A user's **effective permissions** are the distinct union of the permissions inherited from their
**active** roles and the permissions granted **directly** to the user. The resolution query is
`GetPermissionNamesByUserIdQuery` (`src/Application/Modules/Permissions/Queries/`) backed by
`IPermissionRepository.GetPermissionNamesByUserIdAsync`. Because it is the single resolution point,
every emission path (admin cookie, access token, id token, `userinfo`) automatically reflects both
sources.

---

## 3. Claim contract

| Claim type | Claim value | Notes |
|---|---|---|
| `permission` | `oropos.sales.read` | One claim per granted permission. Multiple values under one type. |

Defined as `AuthorizationClaimTypes.Permission` in
`src/Shared/Authorization/AuthorizationClaimTypes.cs`.

This follows the standard claim model (a type plus a value), so all of these work as expected:

```csharp
User.HasClaim(AuthorizationClaimTypes.Permission, "oropos.sales.read");
[Authorize(Policy = "perm:oropos.sales.read")]              // server, exact match
```

Because the type is fixed and the value is the permission name, you can also build UI lists:

```csharp
var permissions = User.FindAll(AuthorizationClaimTypes.Permission).Select(c => c.Value);
```

**Matching is exact.** There are no wildcards, prefixes, or globs. `oropos.sales.write` does not
satisfy `oropos.sales.read`.

---

## 4. Scopes

| Scope | Registered in | Client permission to request it | Effect |
|---|---|---|---|
| `permissions` | `OpenIddictServerConfiguration.RegisterScopes` | `scp:permissions` | Adds `permission` claims to the **id token** and **userinfo** (access token always has them). |

`AuthorizationScopes.Permissions` is the shared constant
(`src/Shared/Authorization/AuthorizationScopes.cs`). The seeded admin client is granted
`scp:permissions` by `DatabaseSeeder`.

---

## 5. Token destinations

| Token / endpoint | `permission` claims included? |
|---|---|
| Access token | **Always** |
| Identity token | Only when the `permissions` scope is granted |
| `userinfo` | Only when the access token has the `permissions` scope |

Destination logic lives in `GetDestination.GetDestinations`
(`src/IdentityServer/IdentityServer/Helpers/GetDestination.cs`) and mirrors the existing `roles`
behavior. `userinfo` is implemented in
`AuthorizationController.Userinfo` (`src/IdentityServer/IdentityServer/Controllers/AuthorizationController.cs`).

Why this split: resource servers enforce authorization from the **access token**; the id token is
about authenticating the user to the client. Putting permissions in the id token or userinfo by
default would leak authorization data to the front channel for no benefit.

The **admin cookie** (used by the Blazor console) also carries `permission` claims so the UI can
gate content via `PermissionView` / policies.

---

## 6. Where permissions are emitted

| Flow | Location |
|---|---|
| Admin sign-in (cookie) | `AdminPasswordSignInService.BuildPrincipalAsync` |
| `authorize` (fast path / consent accepted) | `AuthorizationController.Authorize`, `.Accept` |
| `token` (code / refresh exchange) | `AuthorizationController.Exchange` |
| `userinfo` | `AuthorizationController.Userinfo` (with `permissions` scope) |

All flows call the shared helper `AuthorizationController.AddPermissionClaimsAsync`, which sends
`GetPermissionNamesByUserIdQuery` and adds `SetClaims(AuthorizationClaimTypes.Permission, names)`.

---

## 7. Enforcing permissions

### 7.1 ASP.NET Core (the IdP itself, or a co-hosted API)

```csharp
app.MapGet("/api/sales/orders", () => Results.Ok(orders))
   .RequireAuthorization("perm:oropos.sales.read");
```

Or declaratively:

```csharp
[Authorize(Policy = "perm:oropos.sales.read")]
public IActionResult GetOrders() => Ok();
```

Policies are built on demand by `PermissionPolicyProvider`
(`src/IdentityServer/IdentityServer/Authorization/PermissionPolicyProvider.cs`) from the prefix
`perm:`, so **no per-permission registration is required** — a permission created at runtime is
immediately usable. The handler (`PermissionAuthorizationHandler`) succeeds only on an exact
`permission` claim match.

### 7.2 Blazor client (admin console)

```razor
<PermissionView Permission="oropos.sales.read">
    <button class="btn btn-primary">Export</button>
</PermissionView>
```

`PermissionView` (`src/IdentityServer/IdentityServer.Client/Components/Shared/PermissionView.razor`)
reads `AuthenticationState` and renders its child content only when the claim is present. For
policy-based `AuthorizeView` inside the Blazor app, use the same `perm:<name>` policy names.

### 7.3 External resource servers (.NET, Node, …)

Validate the access token, then read the `permission` claims:

- **JWT validation (recommended):** validate signature against the issuer JWKS
  (`/.well-known/openid-configuration` → `jwks_uri`), verify `iss`/`aud`/expiry, then check the
  `permission` claim values.
- **Introspection:** `POST /connect/introspect` with the access token; the response includes the
  token's claims, including `permission` (requires the client to have `ept:introspection`). See
  `examples/NodeJsApiExample/`.

```csharp
// .NET resource server
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.Authority = "https://identity.example.com");

[Authorize] // then inspect User.FindAll("permission")
```

```javascript
// Node resource server (after introspection or local JWT validation)
const perms = decoded.permission ?? [];        // string or string[]
if (![].concat(perms).includes('oropos.sales.read')) return res.status(403).end();
```

---

## 8. Managing role↔permission and user↔permission assignments

- **Role** permissions: `PUT /api/roles/{id}/permissions` (AdminOnly) with body
  `{ "permissionIds": ["<guid>", ...] }`. The request **replaces** the role's complete permission
  set. Command: `SetRolePermissionsCommand` (`src/Application/Modules/Roles/Commands/`). Unknown
  role → `404`; unknown permission id → validation error (`400`), and nothing changes.
  Console: `RoleDetail.razor` shows a checklist of all permissions and saves the selection.
- **Direct user** permissions: `PUT /api/users/{id}/permissions` (Admin/Administrator) with body
  `{ "permissionIds": ["<guid>", ...] }`. The request **replaces** the user's direct permission
  set. Command: `AssignPermissionsToUserCommand` (`src/Application/Modules/Users/Commands/`).
  Unknown user → `404`; unknown permission id → validation error (`400`). Console:
  `UserDetail.razor` shows a direct-permission checklist plus a read-only **effective
  permissions** panel (roles ∪ direct).
- Read a user's effective permission names: `GET /api/users/{id}/effective-permissions`.
- Creating/editing the permission catalogue itself: `POST/PUT/DELETE /api/permissions` (AdminOnly);
  the console page is `/permissions` (Admin/Administrator). System permissions (`IsSystem`) are
  read-only and cannot be deleted.

---

## 9. Integration quickstart for a client application

1. **Register the client** (admin API or console) with the permissions it needs:
   - `ept:authorization`, `ept:token`, `ept:userinfo`, `ept:introspection`, `ept:revocation`
   - `gt:authorization_code`, `gt:refresh_token`
   - `scp:openid`, `scp:profile`, `scp:email`, `scp:roles`, and `scp:permissions` if you need
     permissions in the id token / userinfo
   - Redirect URIs and `ft:pkce` (public clients)
2. **Request scopes** at authorize time:
   `scope = openid profile email roles permissions offline_access`.
3. **Enforce** with the access token's `permission` claims (or policies if co-hosted).
4. **Read user claims** from the access token or `userinfo` (with the `permissions` scope).

The exact registration payload, flows, and endpoint list live in the repository `README.md`
("Integration — Using the Image in Other Projects").

---

## 10. Troubleshooting

| Symptom | Likely cause |
|---|---|
| `permission` claims missing from the access token | User's roles have no permissions assigned, or the role is deactivated. Check `PUT /api/roles/{id}/permissions` and `Role.IsActive`. |
| `permission` claims missing from the id token / userinfo | The client did not request the `permissions` scope, or lacks `scp:permissions`. |
| `perm:x` policy always denies | Exact-match: the claim value must equal `x` character-for-character (permission names are lowercased). `system.*.*` does not match `system.users.read`. |
| Token too large | Many permissions produce many claims. Request `permissions` only when needed; the access token always carries them, so consider trimming role assignments. |
| `invalid_client` / `unauthorized_client` at the token endpoint | This is an OpenIddict **client** permission/grant issue (`gt:`), unrelated to user permissions. |
| `permissionIds` request returns 400 | One or more permission ids do not exist. |

---

## 11. Anti-confusion checklist (read before writing code or docs)

- [ ] Am I talking about a **client** (`ept:`/`gt:`/`rst:`/`scp:`/`ft:` on the application) or a
      **user** (`provider.resource.action` on the domain `Permission`)?
- [ ] User permissions are emitted as `Claim("permission", <name>)` — not as a claim type named
      after the permission, and not as an OAuth scope.
- [ ] Access token always; id token and userinfo only with the `permissions` scope.
- [ ] Server policies are `perm:<exact-permission-name>`; matching is exact, no wildcards.
- [ ] Effective permissions are the union of role-derived and directly-granted (`UserPermission`)
      permissions; assignments can be managed per role (`PUT /api/roles/{id}/permissions`) and per
      user (`PUT /api/users/{id}/permissions`).
