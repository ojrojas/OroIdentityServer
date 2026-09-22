// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OroIdentityServer.Server.Authorization;
using OroIdentityServer.Shared.Authorization;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class PermissionAuthorizationHandlerTests
{
    private static ClaimsPrincipal Principal(params string[] permissions)
    {
        var identity = new ClaimsIdentity(
            permissions.Select(p => new Claim(AuthorizationClaimTypes.Permission, p)),
            authenticationType: "test");
        return new ClaimsPrincipal(identity);
    }

    private static async Task<bool> EvaluateAsync(ClaimsPrincipal user, string requiredPermission)
    {
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(requiredPermission);
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task ExactMatch_Grants()
    {
        Assert.True(await EvaluateAsync(Principal("oropos.sales.read"), "oropos.sales.read"));
    }

    [Fact]
    public async Task DifferentPermission_Denies()
    {
        Assert.False(await EvaluateAsync(Principal("oropos.sales.write"), "oropos.sales.read"));
    }

    [Fact]
    public async Task WildcardLikeValue_DoesNotGrant()
    {
        // Matching is exact: the seeded literal system.*.* must not satisfy a concrete permission.
        Assert.False(await EvaluateAsync(Principal("system.*.*"), "system.users.read"));
    }

    [Fact]
    public async Task NoPermissionClaims_Denies()
    {
        Assert.False(await EvaluateAsync(Principal(), "oropos.sales.read"));
    }

    [Fact]
    public async Task UnauthenticatedPrincipal_Denies()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.False(await EvaluateAsync(anonymous, "oropos.sales.read"));
    }

    [Fact]
    public async Task PolicyProvider_BuildsPermissionPolicyOnDemand()
    {
        var provider = new PermissionPolicyProvider(Options.Create(new AuthorizationOptions()));

        var policy = await provider.GetPolicyAsync(PermissionPolicyProvider.Prefix + "oropos.sales.read");

        Assert.NotNull(policy);
        Assert.Contains(policy!.Requirements, r => r is PermissionRequirement pr && pr.Permission == "oropos.sales.read");
    }

    [Fact]
    public async Task PolicyProvider_NonPermissionPolicy_FallsBackToDefault()
    {
        var provider = new PermissionPolicyProvider(Options.Create(new AuthorizationOptions()));

        Assert.Null(await provider.GetPolicyAsync("AdminOnly"));
    }
}
