## Why

When an application is configured with consent type **explicit** ("consentimiento de aprobación"), `AuthorizationController.Authorize` reaches the `default` branch of its consent-type switch and calls `return View(new AuthorizeViewModel { ... })` (`src/IdentityServer/IdentityServer/Controllers/AuthorizationController.cs:203`). The project is 100% Blazor — there is no `Views` folder and no `.cshtml` anywhere — so that call throws `InvalidOperationException` ("view not found") instead of showing an approval screen. The same happens whenever a client sends `prompt=consent`. The authorize request aborts and the user can never approve or deny access.

## What Changes

- Redirect instead of render a missing MVC view: the `default` branch of the consent-type switch redirects the (already authenticated) user to a new Blazor consent page, preserving the full OpenIddict request in the query string.
- Add a new Blazor page (static SSR) that renders the approval UI using the existing auth-screen styling (`.auth-card`, `LoginLayout`) and the project's `IStringLocalizer` resources (8 languages), showing the calling application name and the requested scopes with their localized display names/descriptions.
- Resolve scope metadata server-side with `IOpenIddictScopeManager` so each requested scope is displayed with its registered name and localized description (unknown scopes fall back to the raw scope name).
- The consent form echoes every OpenIddict request parameter back as hidden inputs (plus an antiforgery token) and posts to `~/connect/authorize`, so the existing `Accept`/`Deny` actions in `AuthorizationController` handle approval and denial unchanged.
- `Accept` keeps creating a permanent authorization, so a user who approves once is not asked again for the same client + scopes (existing fast-path already skips the screen once a matching permanent authorization exists).
- The screen is shown when consent is **explicit** and no valid permanent authorization covers the requested scopes, and when the client sends `prompt=consent`; it is not shown for `implicit` (fast-path preserved) nor for `external` (sysadmin-granted, existing behavior preserved). **BREAKING** for the (unreachable) `AuthorizeViewModel` path: the `View()` call disappears and `AuthorizeViewModel` becomes dead code to be removed.
- Add localized resource keys (consent title, subtitle, scope list heading, accept/deny buttons, remember-notice) to the shared resource files.

## Capabilities

### New Capabilities

- `consent/approval-consent-screen`: The authorization endpoint SHALL route explicit-consent requests to an interactive approval screen that lists the client application and requested scopes, and SHALL honor the user's approval/denial by completing or aborting the authorization request with the original OIDC parameters preserved.

### Modified Capabilities

- None. No existing specs exist for this area; this is the first.

## Impact

- **IdentityServer**: `AuthorizationController.cs` — `default` branch redirects to the consent page; `AuthorizeViewModel.cs` removed; no changes to `Accept`/`Deny`.
- **IdentityServer UI (Blazor server-rendered)**: new page under `Components/Accounts/Pages/` (e.g. `Consent.razor`) with `LoginLayout`; injects `IHttpContextAccessor` (already registered) to enumerate query parameters and `IAntiforgery` to emit the token; injects `IOpenIddictScopeManager` for scope metadata.
- **Shared resources**: new keys in `SharedResources.*.resx` (en, es-419, de, fr, it, pt-BR, ja, zh-Hans).
- **No database/dependency changes**; no new NuGet packages.
- **Tests**: unit/integration tests for the routing decision (explicit without prior authorization, prompt=consent, implicit fast-path, external), scope resolution/fallback, and the accept/deny round-trip.
