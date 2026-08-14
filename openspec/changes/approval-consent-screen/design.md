## Context

See `proposal.md` — Why. Current state: `AuthorizationController.Authorize` (src/IdentityServer/IdentityServer/Controllers/AuthorizationController.cs) switches on the application's consent type. The `implicit`/`external`/fast-path branches issue tokens directly; the `default` branch (explicit consent without a prior permanent authorization, or any consent type with `prompt=consent`) calls `return View(new AuthorizeViewModel { ... })`. There is no `Views` folder and no `.cshtml` anywhere in the project — the UI is Blazor Server/WASM (Login/Logout/ChangePassword live under `Components/Accounts/Pages`, use `LoginLayout`, `IStringLocalizer<SharedResources>`, and the `.auth-card` CSS in `wwwroot/app.css`). `AddControllersWithViews()` is registered but the controller's `Accept`/`Deny` POST actions are fully functional; only the GET-rendered consent form is missing.

The existing POST actions already implement the whole consent decision: `Accept` builds the identity, creates a permanent authorization (so future requests with the same scopes skip consent), and signs in; `Deny` returns Forbid so OpenIddict replies `access_denied`. Both require a valid antiforgery token (`[ValidateAntiForgeryToken]`). `app.UseAntiforgery()` is in the pipeline and `IHttpContextAccessor` is registered.

## Goals / Non-Goals

**Goals:**
- Make the explicit-consent flow render a working approval screen consistent with the existing Blazor auth pages (same layout, styling, and localization).
- Preserve all OIDC request parameters (state, nonce, PKCE, redirect_uri, ...) across the screen so the existing `Accept`/`Deny` logic and OpenIddict complete the request exactly as if the form had been posted directly.
- Reuse the existing `Accept`/`Deny` actions and permanent-authorization behavior without modifying them.

**Non-Goals:**
- Per-scope checkboxes / letting the user grant a subset of scopes. The approved set is the requested set (matches current `identity.SetScopes(request.GetScopes())` semantics).
- Changing `implicit` or `external` consent behavior.
- Building a generic OIDC consent framework or a dynamic view engine.

## Decisions

### D1: Blazor page (static SSR) instead of an MVC `.cshtml` view
The `default` branch redirects to a new page `Components/Accounts/Pages/Consent.razor` at route `/Account/Consent`, rendered with the default static SSR mode (no `@rendermode`), using `LoginLayout`.

- **Why**: The project has zero Razor views; introducing MVC views would add a second rendering stack, its own layout wiring, and divergent localization. A Blazor page reuses `LoginLayout`, `IStringLocalizer<SharedResources>`, and the `.auth-card` CSS exactly like `Login.razor`/`Logout.razor`. Static SSR (no interactivity) is required so the component can read `HttpContext.Request.Query` and post a plain HTML form to `~/connect/authorize` — interactive render modes do not expose `HttpContext` to components.
- **Alternative rejected**: rendering the consent page with a Blazor interactive render mode and resolving the OIDC parameters via `[SupplyParameterFromQuery]` — requires binding an unbounded, unknown set of OIDC parameters and complicates the POST; enumeration of `HttpContext.Request.Query` is faithful and matches the standard OpenIddict sample pattern.

### D2: Carry the OIDC request in the query string, not TempData/state
`Authorize`'s `default` branch returns `Redirect("/Account/Consent" + Request.QueryString)`. `Consent.razor` enumerates `HttpContext.Request.Query` and emits one hidden input per parameter (`name` + `value`) inside its POST form, plus the antiforgery token.

- **Why**: Stateless and lossless — every original parameter (including ones the page doesn't know about) round-trips. `Request.QueryString` at the authorize endpoint already contains `client_id`, `redirect_uri`, `response_type`, `scope`, `state`, `nonce`, `code_challenge`, `code_challenge_method`, `prompt`, etc.
- **Alternative rejected**: storing parameters in `TempData` keyed by an id and passing only the id — adds server state, expires, and breaks if the user reloads or opens the page in another tab.

### D3: Antiforgery token rendered from the `IAntiforgery` service
`Consent.razor` injects `IAntiforgery` + `IHttpContextAccessor`, calls `Antiforgery.GetAndStoreTokens(httpContext)` in `OnInitialized`, and renders `RequestToken` as a hidden input named `__RequestVerificationToken`. The form posts to `~/connect/authorize`, whose `Accept`/`Deny` actions carry `[ValidateAntiForgeryToken]`.

- **Why**: `@Html.AntiForgeryToken()` is MVC-only; the service call is the Blazor equivalent and works with the existing antiforgery middleware and attribute.

### D4: Scope metadata resolved server-side via `IOpenIddictScopeManager`
The page parses the `scope` parameter (space-separated) and, for each name, calls `ScopeManager.FindByNameAsync(name)` (injected, same process, already registered by `AddOpenIddict().AddCore()`). For a found descriptor it renders `GetLocalizedDisplayName()`/`GetLocalizedDescription()`; otherwise it falls back to the raw scope name.

- **Why**: The page runs in the server process, so the typed scope manager is directly available and stays in sync with the DB without an extra API hop. Unknown scopes (e.g. a custom `admin` scope without metadata) still display by name.

### D5: Controller change is minimal and localized
Only the `default` case of the consent-type switch changes from `return View(...)` to the redirect. All other branches (implicit fast-path, external, `prompt=none` errors) stay untouched. `AuthorizeViewModel.cs` is deleted as dead code.

- **Why**: Keeps the change reviewable and avoids disturbing the existing `prompt=none` and `external` semantics.

### D6: Resource keys added to the shared resx files
New keys (`ConsentTitle`, `ConsentSubtitle`, `ConsentApplicationLabel`, `ConsentScopeHeading`, `ConsentRememberNotice`, `Accept`, `Deny`) added to `src/Shared/Resources/SharedResources.resx` and its culture variants (es-419, de, fr, it, pt-BR, ja, zh-Hans). `LoginLayout`'s language picker keeps the screen translatable.

## Risks / Trade-offs

- **Antiforgery token + static SSR quirks** → `GetAndStoreTokens` persists the token cookie on the GET; the POST to `/connect/authorize` sends both the cookie and the hidden token, which the attribute validates. Covered by an integration test (submit without token → rejected).
- **`/Account` is exempt from the change-password redirect middleware** → the consent screen is reachable even when a user has `MustChangePassword`; acceptable because `Accept` only completes the authorization and does not skip `ChangePassword` flows (existing behavior unchanged).
- **Reload/tab-twice on the consent page** → the page is stateless (query string only), so reload simply re-renders the same form; no double-approval risk because the permanent-authorization check happens in `Accept`/`Authorize`, not in the page.
- **Scope enumeration cost** → one `FindByNameAsync` per requested scope on a server-rendered page; negligible (scopes are few), and results can be cached later if profiling shows a need.
- **Redirect exposes the OIDC request in the browser address bar** → already inherent to the authorize endpoint; no new sensitive data beyond what the client sent.

## Migration Plan

- No database changes, no new dependencies. Deploy the IdentityServer project; rollback is a revert of the change (the old behavior — exception on explicit consent — was broken, so there is no working state to restore).

## Open Questions

- None that affect the specs, approach, or task breakdown. Whether the admin UI should also expose the `systematic` consent type is a separate feature.
