// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.AspNetCore.Authorization;
using OroIdentityServer.Shared.Authorization;

namespace OroIdentityServer.Server.Authorization;

/// <summary>
/// Grants a <see cref="PermissionRequirement"/> when the principal holds a <c>permission</c>
/// claim with the exact required value.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim(AuthorizationClaimTypes.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
