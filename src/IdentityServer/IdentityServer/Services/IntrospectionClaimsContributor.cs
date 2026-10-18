// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
namespace IdentityServer.Services;

using System.Collections.Immutable;
using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

/// <summary>
/// Adds the token's application claims to the RFC 7662 introspection response.
/// </summary>
public interface IIntrospectionClaimsContributor
{
    /// <summary>
    /// Copies the non-protocol claims of an already-resolved token principal into the response
    /// being assembled. Existing entries are left untouched.
    /// </summary>
    void Contribute(ClaimsPrincipal? tokenPrincipal, IDictionary<string, OpenIddictParameter> response);
}

/// <summary>
/// Makes introspection answer with everything the token actually says.
/// </summary>
/// <remarks>
/// OpenIddict's built-in <c>AttachApplicationClaims</c> handler drops the application claims
/// (<c>role</c>, <c>permission</c>, <c>tenant_id</c>, <c>is_master_admin</c>) unless the
/// introspecting client is explicitly listed as an <b>audience</b> of the token, and it logs
/// "Potentially sensitive application claims were excluded ...". That makes the documented
/// integration pattern - a resource server introspects and then reads the <c>permission</c> claim -
/// impossible unless the resource server is also registered as a resource of every scope the
/// frontend requests.
/// This contributor re-adds them, which moves the trust boundary entirely to
/// <c>ept:introspection</c>: OpenIddict still authenticates the caller and still rejects one that
/// lacks that permission, so only a confidential, server-side client - the kind an administrator has
/// explicitly trusted - ever sees the claims.
/// <c>active</c> and the protocol claims are deliberately left alone: OpenIddict computes them
/// (including "this token is revoked" and "this token expired") and reimplementing that would only
/// risk getting revocation wrong.
/// </remarks>
public sealed class IntrospectionClaimsContributor : IIntrospectionClaimsContributor
{
    /// <summary>
    /// Claims the protocol handlers already attach. Re-emitting them would either duplicate values
    /// or replace a well-typed value (exp/iat/nbf are numbers) with a string.
    /// </summary>
    private static readonly HashSet<string> ProtocolClaims = new(StringComparer.Ordinal)
    {
        Claims.Issuer,
        Claims.Subject,
        Claims.Audience,
        Claims.ExpiresAt,
        Claims.IssuedAt,
        Claims.NotBefore,
        Claims.JwtId,
        Claims.ClientId,
        Claims.Scope,
        Claims.TokenType,
        Claims.TokenUsage,
        Claims.Confirmation,
        Claims.Username,
        Claims.AuthenticationMethodReference,
        Claims.AuthenticationContextReference
    };

    public void Contribute(ClaimsPrincipal? tokenPrincipal, IDictionary<string, OpenIddictParameter> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (tokenPrincipal is not { Identity.IsAuthenticated: true })
        {
            return;
        }

        foreach (var grouping in tokenPrincipal.Claims.GroupBy(claim => claim.Type, StringComparer.Ordinal))
        {
            if (ProtocolClaims.Contains(grouping.Key) || response.ContainsKey(grouping.Key))
            {
                continue;
            }

            var values = grouping.Select(claim => (string?)claim.Value).ToList();

            // A single value stays a scalar (clients read "tenant_id" as a string); several values
            // become the ImmutableArray<string> form that OpenIddictParameter serialises as a JSON
            // array, which is the shape "permission" and "role" are read as.
            response[grouping.Key] = values.Count == 1
                ? values[0]
                : ImmutableArray.CreateRange(values);
        }
    }
}

/// <summary>
/// Bridges the contributor into OpenIddict's introspection pipeline, after the built-in handlers
/// have resolved the token and populated the response's protocol claims.
/// </summary>
public sealed class IntrospectionClaimsOpenIddictHandler(IIntrospectionClaimsContributor contributor)
    : IOpenIddictServerHandler<OpenIddictServerEvents.HandleIntrospectionRequestContext>
{
    public ValueTask HandleAsync(OpenIddictServerEvents.HandleIntrospectionRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        contributor.Contribute(context.GenericTokenPrincipal, context.Claims);

        return default;
    }
}
