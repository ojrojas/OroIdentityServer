// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;

/// <summary>
/// Gives a token-authenticated principal the same claim vocabulary as the admin cookie.
/// </summary>
/// <remarks>
/// The admin cookie is built with the long <see cref="ClaimTypes"/> names
/// (<c>ClaimTypes.NameIdentifier</c>, <c>ClaimTypes.Name</c>, <c>ClaimTypes.Role</c>) and a large
/// part of the codebase reads those. An OIDC access token carries the short OIDC names
/// (<c>sub</c>, <c>name</c>, <c>role</c>) and must not be claim-mapped, or <c>sub</c> and
/// <c>preferred_username</c> would collide on one long name. So instead of mapping the token, the
/// short claims are copied onto their long aliases and the identity is re-issued with the long
/// name/role types - which is also what makes <c>RequireRole(...)</c> and <c>IsInRole(...)</c>
/// work for a bearer call.
/// Idempotent by construction: the aliases are only added when absent.
/// </remarks>
public sealed class BearerClaimAliasTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Keyed on the shape rather than on the scheme name: OpenIddict stamps its identities with
        // TokenValidationParameters.DefaultAuthenticationType, not with the scheme name. A cookie
        // principal already carries the long names and is returned untouched.
        if (principal.Identity?.IsAuthenticated != true
            || principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
        {
            return Task.FromResult(principal);
        }

        var transformed = new List<Claim>();
        var seen = new HashSet<string>(principal.Claims.Select(c => c.Type), StringComparer.Ordinal);

        void AddAlias(string fromType, string toType)
        {
            if (seen.Contains(toType)) return;

            foreach (var claim in principal.Claims.Where(c => c.Type == fromType))
            {
                transformed.Add(new Claim(toType, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer));
            }
        }

        AddAlias(Claims.Subject, ClaimTypes.NameIdentifier);
        AddAlias(Claims.Name, ClaimTypes.Name);
        AddAlias(Claims.Role, ClaimTypes.Role);
        AddAlias(Claims.Email, ClaimTypes.Email);

        if (transformed.Count == 0)
        {
            return Task.FromResult(principal);
        }

        var identities = principal.Identities
            .Select(identity => new ClaimsIdentity(
                identity.Claims.Concat(transformed),
                identity.AuthenticationType,
                ClaimTypes.Name,
                ClaimTypes.Role))
            .ToList();

        return Task.FromResult(new ClaimsPrincipal(identities));
    }
}