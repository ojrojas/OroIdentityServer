// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
namespace IdentityServer.Services;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OroIdentityServer.Server.Helpers;
using static OpenIddict.Abstractions.OpenIddictConstants;

/// <summary>
/// Builds the claims identities OpenIddict turns into tokens, for the grants that are not backed
/// by a signed-in user.
/// </summary>
public interface ITokenPrincipalFactory
{
    /// <summary>
    /// The identity for a machine-to-machine token: the client is the subject because there is no
    /// resource owner. It carries no role, permission or master-admin claim - see the remarks on
    /// the implementation for why that is the security boundary of the grant.
    /// </summary>
    ClaimsIdentity CreateClientCredentialsIdentity(string clientId);

    /// <summary>
    /// Resolves the requested scopes into their resources/audience and stamps every claim with the
    /// destinations that decide whether it reaches the access token or the identity token.
    /// </summary>
    Task ApplyScopesAndDestinationsAsync(
        ClaimsIdentity identity,
        IEnumerable<string> requestedScopes,
        CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class TokenPrincipalFactory(IOpenIddictScopeManager scopeManager) : ITokenPrincipalFactory
{
    /// <summary>
    /// A client_credentials token deliberately carries none of the user entitlements. The admin API
    /// authorises on roles (<c>ManagerOrAdmin</c>, <c>MasterAdminOnly</c>) which only ever come from
    /// a user's catalogue membership, so a machine token cannot reach it. What the token can do is
    /// expressed by its scopes and its audience, which is what an external resource server should
    /// check.
    /// </summary>
    public ClaimsIdentity CreateClientCredentialsIdentity(string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // OpenIddict requires a non-empty subject to keep the issued entry addressable (and the
        // token-entry validation added for revocation resolution looks it up by subject).
        identity.SetClaim(Claims.Subject, clientId)
                .SetClaim(Claims.ClientId, clientId);

        return identity;
    }

    public async Task ApplyScopesAndDestinationsAsync(
        ClaimsIdentity identity,
        IEnumerable<string> requestedScopes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identity);

        identity.SetScopes(requestedScopes);
        identity.SetResources(await scopeManager
            .ListResourcesAsync(identity.GetScopes(), cancellationToken)
            .ToListAsync(cancellationToken));

        var principal = new ClaimsPrincipal(identity);
        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(GetDestination.GetDestinations(principal, claim).ToArray());
        }
    }
}
