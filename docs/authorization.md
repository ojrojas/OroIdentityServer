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
- **Introspection:** `POST /connect/introspect` with the access token; the response mirrors the
  token, including `role`, `permission` and `tenant_id` (requires the client to have
  `ept:introspection`). See `examples/NodeJsApiExample/`.

  **`ept:introspection` is the whole trust boundary.** A client granted that permission sees the
  token's claims, so grant it only to confidential, server-side clients. OpenIddict withholds the
  application claims from anyone who is not an explicit audience of the token; this server
  deliberately re-adds them (see `IntrospectionClaimsContributor`), because the documented pattern -
  introspect, then read `permission` - would otherwise require registering every resource server as
  a resource of every scope the frontend requests.

  Two behaviours are inherited from OpenIddict and worth knowing:

  | Situation | Response |
  |---|---|
  | Caller lacks `ept:introspection`, token resolves | `400 unauthorized_client` |
  | Caller lacks `ept:introspection`, token does **not** resolve | `200 {"active": false}` |
  | Caller has `ept:introspection`, token resolves | `200` with the token's claims |
  | Caller has `ept:introspection`, token revoked/expired/unknown | `200 {"active": false}` |

  The third row of the second and third lines is the odd one: an unauthorised client learns that a
  token is unusable. It leaks nothing (the caller already holds the token), and it is OpenIddict
  resolving the token during request extraction, before any validation runs - this version exposes no
  introspection passthrough to intercept it. `IntrospectionClaimsTests` in
  `tests/Server.Tests/Endpoints/` pins the rows that matter.

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

## 9.0 Machine-to-machine clients (`client_credentials`)

For a service that acts on its own behalf there is no user and therefore no browser step:

```bash
curl -X POST https://identity.example.com/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=my-service&client_secret=…&scope=oropos.api"
```

Register the client as **confidential**, with `ept:token`, `gt:client_credentials` and
`scp:<scope>` for every scope it may request. `consentType` is irrelevant to this grant: consent is
an authorization-endpoint concept and there is nobody to approve anything, so `implicit`,
`explicit` and `external` behave identically (`ClientCredentialsGrantTests` covers all three).

What a machine token is and is not:

- `sub` is the **client id**, not a user id. There is no `id_token` and no `refresh_token` (the
  service can mint a new token from its own credentials at any time).
- It carries **no** `role`, `permission`, `tenant_id` or `is_master_admin`: those come from a user's
  catalogue membership. It therefore cannot reach the admin API, whose policies are role-based
  (`403`), and `/connect/userinfo` answers with an error rather than inventing a profile.
- Authorise it on **scopes and audience** (`scp` claims / `aud`), which is what an external
  resource server should check. See §7.3 for the introspection audience requirement, which applies
  to these tokens too.

---

## 9.1 What a client sees when the user has MFA enabled

MFA lives **entirely inside the interactive login**, so a client application does not have to
implement anything — but it does have to tolerate one extra user-facing step:

1. The browser posts to `/auth/login`. With a second factor enabled, no token is issued and no
   `UserSession` is created; the server stores a short-lived, single-use challenge and redirects
   the browser to its own `/Account/Mfa` page.
2. The user enters the authenticator code (or a recovery code) there, and only then is the
   `oro.identity.admin` cookie issued — the same cookie an RP-initiated `connect/authorize`
   accepts. From that point on the flow is unchanged.
3. If the user has no factor yet and the tenant requires one, the login is routed through
   `/Account/MfaSetup` (QR + manual key) instead.

Consequences worth knowing:

- No new parameter, claim or scope is involved; the token contents are identical whether MFA was
  used or not. Tokens do **not** currently carry `amr`/`acr`.
- `prompt=none` still answers `login_required` for an unauthenticated browser; a silent renew
  cannot cross the MFA step, by design.
- Clients must treat "redirect to an IdentityServer-owned page" as normal. Do not hard-code
  assumptions about the number of redirects during a login.
- An expired admin session now redirects to `/Account/Login?reason=expired&ReturnUrl=...` instead
  of a bare login URL, and `401` responses from `/api/*` carry a `Location` header pointing at the
  same page. Web clients can use that header to send the user back to the login page; the bundled
  admin console does it through a `DelegatingHandler`.

---

## 9.2 The two bundled `oroidentity-admin` examples (implicit vs explicit consent)

The repository seeds the **same** Angular SPA twice, differing only in `consentType`, so the two
behaviours can be compared side by side:

| Client id | `consentType` | Dev server | What `/connect/authorize` does after login |
|---|---|---|---|
| `OroIdentityServer.Admin` | `implicit` | `http://localhost:4200` (Aspire publishes `30645`) | Returns the `code` immediately. |
| `OroIdentityServer.Admin.Explicit` | `explicit` | `http://localhost:4201` (Aspire publishes `30646`) | Redirects to `/Account/Consent`, then returns the `code` once approved. |

`aspire run` starts both; each has its own generated environment file and build configuration
(`development-explicit`) because two dev servers sharing one generated file would race on
`CLIENT_ID`. Outside Aspire, `pnpm start:explicit` runs the second one on its own.

**Which one to use.** `implicit` is right for a first-party SPA: a consent screen would interrupt
the silent renew flow on every fresh authorization and the SPA is already trusted. `explicit` is
what you want when a resource has to be brought *under the identity server's control* rather than
trusted by default: the user approves which scopes the client may use, and that approval becomes a
permanent authorization recorded against the user and the client, so it is asked for once and then
remembered (`ConsentFlowTests` covers approve, deny, re-prompt with `prompt=consent`, and
`consent_required` for `prompt=none`).

Three things are easy to get wrong here:

- `consentType` is a property of the **client**, not of the request. An implicit client never shows
  the screen even if the user has never approved anything.
- `external` is a third value: it never shows a screen and instead requires a permanent
  authorization to already exist (granted out of band). It is not a synonym for `explicit`.
- Consent is an **authorization-endpoint** concept, so it is irrelevant to `client_credentials`
  (§9.0): all three values behave identically for that grant.

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
| Login redirects to `/Account/Mfa` instead of the application | The user has MFA enabled; the second factor is cleared in the browser, not by the client. |
| A freshly enrolled code is rejected right after confirming enrolment | Confirming burns that 30-second time step (anti-replay). Use the next code the app shows. |
| `Unprotect` of a TOTP secret fails at runtime | The Data Protection key ring (`data-protection-keys/`) changed or was not persisted; without it every enrolled authenticator is unusable. |
| An access token still works after logout / "sign out everywhere" | Should not happen: validation runs with `EnableTokenEntryValidation()`, so `/api` re-checks the token entry and returns 401 once it is revoked. Introspection agrees. If a resource server accepts it, that server is validating offline (JWKS) and can only bound the window by the token's `exp`. |
| Introspection returns `active: false` for a token the caller was just issued | The caller is neither the token's client nor one of its audiences and lacks `ept:introspection`. See §7.3. |
| Introspection returns `active: true` but no `permission` claim | The caller is not an explicit audience of the token and this server's contributor did not run: check that `IntrospectionClaimsContributor` is registered (`IIntrospectionClaimsContributor`) and that the token actually carries the claim (the access token always does). |
| A `client_credentials` token is refused by `/api` with 403 | Expected. A machine token carries no user roles, and the admin API authorises on roles. Use scopes/`aud` in the resource server instead. |
| `/connect/userinfo` returns an error for a `client_credentials` token | Expected: there is no end user to describe, so the endpoint refuses rather than inventing an identity. |

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
