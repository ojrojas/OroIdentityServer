// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Validation.AspNetCore;

/// <summary>
/// The scheme <c>/api</c> requests are authenticated with when they carry a bearer token.
/// </summary>
/// <remarks>
/// The web console (Blazor) authenticates with the <c>oro.identity.admin</c> cookie and never
/// sends a token; the Angular <c>oroidentity-admin</c> SPA authenticates with the OIDC access
/// token and never sends a cookie. Both call the same <c>/api</c> endpoints, so a single default
/// scheme cannot serve both - and cookie-only is precisely why every request from the SPA came
/// back <c>401 {"error":"unauthorized"}</c> even holding a valid access token.
/// The selector therefore looks for a bearer <c>Authorization</c> header, and only for <c>/api</c>,
/// so the interactive login pages keep falling through to the cookie.
/// </remarks>
public static class AdminOrBearerSchemeSelector
{
    public const string SchemeName = "AdminOrBearer";

    public static string Select(HttpContext context)
    {
        if (IsApiRequest(context.Request)
            && context.Request.Headers.Authorization.Count > 0
            && context.Request.Headers.Authorization[0]!.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Registered by OpenIddict's .AddValidation(...).UseAspNetCore(), which validates this
            // server's own access tokens against the same signing keys it issued them with.
            return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        }

        return CookieAuthHandlerSetup.AdminScheme;
    }

    private static bool IsApiRequest(HttpRequest request) =>
        request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}