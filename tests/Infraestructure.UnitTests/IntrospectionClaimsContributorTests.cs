// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
using System.Collections.Immutable;
using System.Security.Claims;
using IdentityServer.Services;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentityServer.Infraestructure.UnitTests;

/// <summary>
/// The contributor that re-adds the token's application claims to the introspection response.
/// This is where the "introspect then read the permission claim" contract is actually implemented,
/// so the claim shapes are pinned here rather than only through the integration tests.
/// </summary>
public class IntrospectionClaimsContributorTests
{
    private static readonly IIntrospectionClaimsContributor Contributor = new IntrospectionClaimsContributor();

    private static ClaimsPrincipal Token(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(
            claims.Select(claim => new Claim(claim.Type, claim.Value)),
            authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void Contribute_AddsTheApplicationClaimsAndLeavesTheProtocolClaimsToOpenIddict()
    {
        // OpenIddict's own handlers attach iss/sub/exp/... before this contributor runs, so it must
        // not duplicate them - a second "sub" would either be dropped or replace a well-typed value.
        var response = new Dictionary<string, OpenIddictParameter>
        {
            [Claims.Subject] = "user-1",
            [Claims.JwtId] = "token-id"
        };

        Contributor.Contribute(
            Token(
                (Claims.Subject, "user-1"),
                (Claims.JwtId, "token-id"),
                ("role", "Administrator"),
                ("permission", "oropos.sales.read"),
                ("tenant_id", "tenant-1")),
            response);

        Assert.Equal("user-1", response[Claims.Subject].ToString());
        Assert.Equal("token-id", response[Claims.JwtId].ToString());
        Assert.Equal("Administrator", response["role"].ToString());
        Assert.Equal("oropos.sales.read", response["permission"].ToString());
        Assert.Equal("tenant-1", response["tenant_id"].ToString());
    }

    [Fact]
    public void Contribute_DoesNotInventProtocolClaims()
    {
        var response = new Dictionary<string, OpenIddictParameter>();

        Contributor.Contribute(Token((Claims.Subject, "user-1")), response);

        Assert.DoesNotContain(Claims.Subject, response.Keys);
    }

    [Fact]
    public void Contribute_KeepsMultiValuedClaimsAsAnArray()
    {
        var response = new Dictionary<string, OpenIddictParameter>();

        Contributor.Contribute(
            Token(("permission", "a.read"), ("permission", "b.write")),
            response);

        // OpenIddict exposes an explicit conversion to ImmutableArray<string> for the array form:
        // that is how a JSON array of values is recognised when the response is serialised.
        var permissions = (ImmutableArray<string?>)response["permission"];

        Assert.Equal(["a.read", "b.write"], permissions.Select(value => value!).ToArray());
    }

    [Fact]
    public void Contribute_AvoidsDuplicatingTheClaimsOpenIddictAlreadyAttached()
    {
        var response = new Dictionary<string, OpenIddictParameter>
        {
            [Claims.Subject] = "user-1",
            ["permission"] = "already-there"
        };

        Contributor.Contribute(Token((Claims.Subject, "user-1"), ("permission", "a.read")), response);

        Assert.Equal("user-1", response[Claims.Subject].ToString());
        Assert.Equal("already-there", response["permission"].ToString());
    }

    [Fact]
    public void Contribute_IgnoresAnUnauthenticatedPrincipal()
    {
        var response = new Dictionary<string, OpenIddictParameter>();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Contributor.Contribute(anonymous, response);

        Assert.Empty(response);
    }

    [Fact]
    public void Contribute_IgnoresAMissingPrincipal()
    {
        var response = new Dictionary<string, OpenIddictParameter>();

        Contributor.Contribute(null, response);

        Assert.Empty(response);
    }

    [Fact]
    public void Contribute_CarriesTheMasterAdminFlagThrough()
    {
        // is_master_admin is what a co-hosted API uses to answer "is this the IdP owner", so it has
        // to survive introspection just like role and permission do.
        var response = new Dictionary<string, OpenIddictParameter>();

        Contributor.Contribute(Token(("is_master_admin", "true")), response);

        Assert.Equal("true", response["is_master_admin"].ToString());
    }
}
