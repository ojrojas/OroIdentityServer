## 1. Authorization endpoint routing

- [x] 1.1 In `src/IdentityServer/IdentityServer/Controllers/AuthorizationController.cs`, replace the `default:` case of the consent-type switch (currently `return View(new AuthorizeViewModel {...})`) with `return Redirect("/Account/Consent" + Request.QueryString)` so the full OIDC request is preserved.
- [x] 1.2 Confirm the other branches are untouched: `implicit`, `external` (with/without authorizations), explicit-with-matching-authorization fast-path, and the `prompt=none` `consent_required` errors.
- [x] 1.3 Remove now-unused imports/usings from the controller (e.g. the `AuthorizeViewModel` import) and verify the `Accept`/`Deny` actions still compile unchanged.

## 2. Consent Blazor page

- [x] 2.1 Create `src/IdentityServer/IdentityServer/Components/Accounts/Pages/Consent.razor` with `@page "/Account/Consent"`, `@layout LoginLayout`, and the `[AllowAnonymous]` attribute (matching `Login.razor`).
- [x] 2.2 Inject `IHttpContextAccessor`, `IAntiforgery`, `IOpenIddictScopeManager`, and `IStringLocalizer<SharedResources>`; keep the page in static SSR (no `@rendermode`).
- [x] 2.3 In the component, enumerate `HttpContext.Request.Query` and keep every parameter so the form can echo them as hidden inputs.
- [x] 2.4 Call `Antiforgery.GetAndStoreTokens(httpContext)` and render the request token as a hidden `__RequestVerificationToken` input.
- [x] 2.5 Parse the `scope` query parameter and resolve each scope with `IOpenIddictScopeManager.FindByNameAsync`, producing a display list of (name, localized display/description) with fallback to the raw scope name when the scope is unknown or has no metadata.
- [x] 2.6 Resolve the client application display name (e.g. via `IOpenIddictApplicationManager.FindByClientIdAsync` + `GetLocalizedDisplayNameAsync`) for the screen header.
- [x] 2.7 Render the approval UI using the existing `.auth-card`/`.auth-form` markup: application name, requested scopes list, and two submit buttons (`submit.Accept` and `submit.Deny`), posting to `/connect/authorize`.
- [ ] 2.8 Verify the rendered HTML contains every original query parameter as a hidden field plus the antiforgery token.

## 3. Localization resources

- [x] 3.1 Add the consent keys (`ConsentTitle`, `ConsentSubtitle`, `ConsentApplicationLabel`, `ConsentScopeHeading`, `ConsentRememberNotice`, `Accept`, `Deny`) to `src/Shared/Resources/SharedResources.resx`.
- [x] 3.2 Add the same keys to every culture variant: es-419, de, fr, it, pt-BR, ja, zh-Hans (en remains the fallback).

## 4. Cleanup

- [x] 4.1 Delete `src/IdentityServer/IdentityServer/ViewModels/AuthorizeViewModel.cs` (now dead code) and confirm nothing else references `AuthorizeViewModel`.

## 5. Tests

- [ ] 5.1 Add integration tests in `tests/Server.Tests/Endpoints/ConsentFlowTests.cs` using the `AspireTestCollection` fixture.
- [ ] 5.2 Test: explicit-consent app without a prior permanent authorization → `GET /connect/authorize` returns a redirect to `/Account/Consent` carrying the OIDC parameters.
- [ ] 5.3 Test: explicit-consent app with a matching permanent authorization and no `prompt=consent` → the authorization completes directly (no consent redirect).
- [ ] 5.4 Test: `prompt=consent` re-displays the consent screen even when a permanent authorization exists.
- [ ] 5.5 Test: `prompt=none` without an authorization → `consent_required` error, no screen.
- [ ] 5.6 Test: the consent page HTML lists the requested scopes and includes the antiforgery token and hidden OIDC parameters.
- [ ] 5.7 Test: approving (POST `submit.Accept` with a valid token) redirects to the client `redirect_uri` with a `code` and the original `state`, and creates a permanent authorization (follow-up request skips consent).
- [ ] 5.8 Test: denying (POST `submit.Deny`) redirects with `error=access_denied` and the original `state`.
- [ ] 5.9 Test: consent decision submitted without a valid antiforgery token is rejected.

## 6. Build and verification

- [x] 6.1 Build the IdentityServer project (`dotnet build src/IdentityServer/IdentityServer`).
- [ ] 6.2 Run the Server.Tests suite (`dotnet test tests/Server.Tests`).
- [ ] 6.3 Manual end-to-end via the AppHost: configure an explicit-consent client, run the authorize request, approve → app receives the code; repeat authorize → screen skipped; re-run with `prompt=consent` → screen shown again; deny → `access_denied`.
