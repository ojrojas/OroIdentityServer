// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using IdentityServer.Services;
using NSubstitute;
using OpenIddict.Abstractions;
using OroIdentityServer.Shared.Authorization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentityServer.Infraestructure.UnitTests;

/// <summary>
/// The machine-to-machine branch of the token factory. This is where the security boundary of the
/// client_credentials grant lives: the identity must identify the client and must not carry any of
/// the claims the admin API authorises on.
/// </summary>
public class TokenPrincipalFactoryTests
{
    private static TokenPrincipalFactory CreateFactory() => new(Substitute.For<IOpenIddictScopeManager>());

    [Fact]
    public void ClientCredentialsIdentity_IdentifiesTheClientAsTheSubject()
    {
        var identity = CreateFactory().CreateClientCredentialsIdentity("machine-client");

        // OpenIddict needs a subject to keep the issued token entry addressable, and for a machine
        // the client is the only subject there is.
        Assert.Equal("machine-client", identity.FindFirst(Claims.Subject)?.Value);
        Assert.Equal("machine-client", identity.FindFirst(Claims.ClientId)?.Value);
    }

    [Fact]
    public void ClientCredentialsIdentity_CarriesNoUserEntitlements()
    {
        var identity = CreateFactory().CreateClientCredentialsIdentity("machine-client");

        // Roles, permissions, the master-admin flag and the tenant all come from a user's catalogue
        // membership. A machine has none, and the admin API's policies are built on them.
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == ClaimTypes.Role);
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == AuthorizationClaimTypes.Permission);
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == AuthorizationClaimTypes.IsMasterAdmin);
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == AuthorizationClaimTypes.TenantId);
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == Claims.Name);
        Assert.DoesNotContain(identity.Claims, claim => claim.Type == Claims.Email);
    }

    [Fact]
    public void ClientCredentialsIdentity_UsesTheOidcShortClaimNames()
    {
        var identity = CreateFactory().CreateClientCredentialsIdentity("machine-client");

        // A token read by an external client must not leak the long .NET ClaimTypes names.
        Assert.DoesNotContain(identity.Claims, claim => claim.Type.StartsWith("http://schemas.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ClientCredentialsIdentity_RejectsABlankClientId(string clientId)
    {
        // A token with no subject would be unaddressable; failing loudly beats issuing one.
        Assert.Throws<ArgumentException>(() => CreateFactory().CreateClientCredentialsIdentity(clientId));
    }
}
