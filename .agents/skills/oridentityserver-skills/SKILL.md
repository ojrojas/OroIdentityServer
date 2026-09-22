---
name: oridentityserver-skills
description: >
  Guide for integrating authentication and authorization into an application that consumes
  OroIdentityServer (OAuth2 / OpenID Connect via OpenIddict). USE FOR: registering an OIDC
  client, choosing a flow, requesting scopes, reading user claims and domain user permissions,
  validating access tokens (JWKS or introspection), enforcing permission policies, remote
  logout / revocation, and troubleshooting integration errors. DO NOT USE FOR: modifying
  OroIdentityServer's own admin console internals, or configuring OpenIddict *client*
  permissions (ept:/gt:/rst:/scp:/ft:) on an application unless the task is about registering a
  client. Always read docs/authorization.md in the OroIdentityServer repo for the canonical
  contract.
---

# Integrating with OroIdentityServer

OroIdentityServer is an OAuth2 / OpenID Connect provider built on **OpenIddict 8**. This skill
tells an agent how to wire an application (web, SPA, service, or resource API) to it correctly.

Canonical references in the OroIdentityServer repository:

- `docs/authorization.md` — user-permission contract, claim/scope semantics, enforcement, troubleshooting.
- `README.md` → "Integration — Using the Image in Other Projects" — discovery, client registration, flows, endpoints.

Do not duplicate those documents; read them and apply them.

---

## Step 0 — Disambiguate before writing any code

The word **"Permissions"** means two different things. Getting this wrong is the most common failure.

| Term | Meaning | Format | Managed where |
|---|---|---|---|
| **Client permissions** | What the OAuth **client** may do | `ept:*`, `gt:*`, `rst:*`, `scp:*`, `ft:*` | On the application record |
| **User permissions** | What the signed-in **user** may do | `provider.resource.action` (e.g. `oropos.sales.read`) | On roles, via `PUT /api/roles/{id}/permissions` |

Rules:

- A user permission is emitted as `Claim("permission", "<name>")` — **one claim per permission**.
- The access token **always** carries `permission` claims; the id token and `userinfo` carry them
  **only** when the `permissions` scope is requested.
- Server enforcement uses exact-match policies named `perm:<permission-name>`. **No wildcards.**
- Never encode user permissions as OAuth scopes and never treat `ept:`/`gt:` values as user permissions.

---

## Step 1 — Discover the provider metadata

```
GET {issuer}/.well-known/openid-configuration
```

Most libraries can consume this URL directly. Key fields: `authorization_endpoint`,
`token_endpoint`, `userinfo_endpoint`, `jwks_uri`, `introspection_endpoint`,
`end_session_endpoint`, `scopes_supported`, `grant_types_supported`.

---

## Step 2 — Register the client application

Register through the admin API or console. Choose the client permissions for the scenario:

| Scenario | Client permissions to grant |
|---|---|
| Interactive web app (auth code) | `ept:authorization`, `ept:token`, `ept:userinfo`, `ept:end_session`, `gt:authorization_code`, `gt:refresh_token`, `rst:code`, `scp:openid`, `scp:profile`, `scp:email`, `scp:roles`, `scp:permissions`, `ft:pkce` |
| SPA / native (public client) | Same as above, **public** client type, **no secret**, PKCE required |
| Machine-to-machine | `ept:token`, `gt:client_credentials` |
| Resource API that introspects tokens | `ept:introspection` (and `ept:revocation` if it revokes) |

API example:

```bash
curl -X POST https://{host}/api/applications \
  -H "Authorization: Bearer {admin-token}" -H "Content-Type: application/json" \
  -d '{
    "clientId": "my-app",
    "clientSecret": "my-secret",
    "displayName": "My Application",
    "clientType": "confidential",
    "consentType": "implicit",
    "permissions": ["ept:authorization","ept:token","ept:userinfo","gt:authorization_code","gt:refresh_token","rst:code","scp:openid","scp:profile","scp:email","scp:roles","scp:permissions","ft:pkce"],
    "redirectUris": ["https://localhost:5001/signin-oidc"],
    "postLogoutRedirectUris": ["https://localhost:5001/signout-callback-oidc"]
  }'
```

Notes:

- **Confidential** clients authenticate with a secret; **public** clients (SPA/native) must use
  PKCE and must not embed a secret.
- `consentType`: `implicit` completes authorization without a consent screen when an authorization
  exists; `explicit` shows the approval screen; `external` requires a pre-existing sysadmin grant.
- The response may return a masked secret (`sk-****xyz`). Persist the real secret only at creation
  time; never send a masked value back on update.

---

## Step 3 — Choose the flow

| Flow | Grant type | Use case |
|---|---|---|
| Authorization Code + PKCE | `authorization_code` | Interactive web/SPA/native (recommended) |
| Refresh Token | `refresh_token` | Renew access without re-authentication |
| Client Credentials | `client_credentials` | Service-to-service, no user |
| Password | `password` | Legacy / first-party only |

Always use PKCE for public clients. Never ship a client secret in a browser or mobile app.

---

## Step 4 — Configure the application

### .NET interactive app (OpenIddict client)

```csharp
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIddict()
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AddDevelopmentEncryptionAndSigningCertificate();
        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough()
            .EnablePostLogoutRedirectionEndpointPassthrough();
        options.UseSystemNetHttp();
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://identity.example.com/"),
            ClientId = "my-app",
            ClientSecret = "my-secret",
            RedirectUri = new Uri("https://localhost:5001/signin-oidc"),
            PostLogoutRedirectUri = new Uri("https://localhost:5001/signout-callback-oidc"),
            Scopes = { "openid", "profile", "email", "roles", "permissions", "offline_access" }
        });
    });
```

### .NET resource server (validate JWTs locally)

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://identity.example.com/";
        options.Audience = "my-api"; // only if the token sets an audience
        options.RequireHttpsMetadata = true;
    });
```

### SPA (e.g. angular-auth-oidc-client)

```ts
provideAuth({
  config: {
    authority: environment.IDENTITY_SERVER,
    clientId: environment.CLIENT_ID,
    redirectUrl: window.location.origin + '/auth/callback',
    postLogoutRedirectUri: window.location.origin,
    scope: 'openid profile email roles permissions offline_access',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
  }
}, withAppInitializerAuthCheck())
```

### Node / Python

Use any standards-compliant OIDC library pointed at the discovery document
(`authority` / `server_metadata_url`). Do not hand-roll the protocol.

---

## Step 5 — Scopes and claims

| Scope | Grants |
|---|---|
| `openid` | id token (subject) |
| `profile` | name, given_name, family_name, preferred_username |
| `email` | email |
| `roles` | `role` claims in id token / userinfo |
| `permissions` | `permission` claims in id token / userinfo |
| `offline_access` | refresh token |
| `tenant_id` (registered scope) | `tenant_id` claim |

Claim reference:

| Claim | Where | Notes |
|---|---|---|
| `sub` | all | user id |
| `role` | access always; id/userinfo with `roles` | role names |
| `permission` | access always; id/userinfo with `permissions` | domain permission names, exact-match |
| `tenant_id` | access + id | home tenant |

`permission` is a multi-valued claim: read **all** values, not just the first.

---

## Step 6 — Enforce user permissions

### Co-hosted / .NET API

```csharp
endpoints.MapGet("/api/sales/orders", ...)
         .RequireAuthorization("perm:oropos.sales.read");

[Authorize(Policy = "perm:oropos.sales.read")]
public IActionResult Export() => Ok();
```

Policies are created on demand; do not pre-register every permission. Matching is exact — no
wildcards, prefixes, or globs.

### External resource server

1. Validate the access token (JWKS signature, `iss`, `aud`, expiry) **or** call
   `POST /connect/introspect` (requires `ept:introspection` on the client).
2. Read the `permission` claim(s) and compare exactly.

```csharp
var permissions = User.FindAll("permission").Select(c => c.Value);
if (!permissions.Contains("oropos.sales.read")) return Forbid();
```

```javascript
const permissions = [].concat(decoded.permission ?? []);
if (!permissions.includes('oropos.sales.read')) return res.status(403).end();
```

### Blazor console (inside OroIdentityServer)

```razor
<PermissionView Permission="oropos.sales.read">
    <button class="btn btn-primary">Export</button>
</PermissionView>
```

---

## Step 7 — Logout, revocation, remote logout

- RP-initiated logout: redirect to `{issuer}/connect/logout` with `id_token_hint` and
  `post_logout_redirect_uri`.
- Token revocation: `POST /connect/revoke`.
- **Remote logout detection:** an API introspects the access token; when an admin ends the
  session server-side, introspection returns `active: false` and the API returns `401`, prompting
  the client to sign out. See `examples/NodeJsApiExample/`.

---

## Step 8 — Security checklist

- [ ] PKCE enabled for every public client; secrets only on confidential clients.
- [ ] Redirect URIs are absolute, HTTPS in production, and registered exactly (no wildcards).
- [ ] Validate issuer, audience (if used), signature, and expiry of access tokens.
- [ ] Request the minimum scopes; request `permissions` only when the frontend needs them.
- [ ] Never trust `permission`/`role` values from the browser without validating the token.
- [ ] Do not log tokens or secrets.
- [ ] Use the discovery document; do not hardcode endpoint paths.

---

## Step 9 — Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `invalid_client` | Wrong client id/secret, or a public client sending a secret. |
| `unauthorized_client` / `invalid_grant` at token | Missing `gt:*` client permission for the flow. |
| Consent screen loops | `consentType=external` without a grant, or `prompt=consent`; use `implicit`/`explicit` appropriately. |
| `invalid_redirect_uri` | Redirect URI not registered or not an exact match. |
| `permission` claims absent | User's roles have no permissions, role deactivated, or (for id/userinfo) the `permissions` scope was not requested / `scp:permissions` not granted. |
| `perm:x` always denies | Exact-match: names are lowercased; `system.*.*` does not match concrete permissions. |
| `401` after admin logout | Expected: token revoked; handle by signing the user out. |
| CORS errors from SPA | Ensure the origin/redirect URI is registered; the server allows CORS for API calls. |

---

## Definition of done for an integration task

- [ ] Client registered with the minimum required client permissions and exact redirect URIs.
- [ ] App uses the discovery document and a standards-compliant OIDC library.
- [ ] PKCE for public clients; no secret in browser/mobile.
- [ ] Token validation configured (JWKS or introspection) with issuer/audience/expiry checks.
- [ ] User permissions enforced via `permission` claims / `perm:` policies (exact match).
- [ ] Logout and remote-logout handling implemented.
- [ ] Scopes requested are the minimum needed (`permissions` only if the frontend consumes them).
