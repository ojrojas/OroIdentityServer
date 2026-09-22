// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace OroIdentityServer.Server.Authorization;

/// <summary>
/// Builds permission policies on demand from a name of the form <c>perm:&lt;permission-name&gt;</c>,
/// so callers can write <c>[Authorize(Policy = "perm:oropos.sales.read")]</c> without
/// registering each permission at startup. Any other policy name falls back to the default
/// <see cref="DefaultAuthorizationPolicyProvider"/> behavior.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string Prefix = "perm:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[Prefix.Length..].Trim();
            if (!string.IsNullOrWhiteSpace(permission))
            {
                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();
            }
        }

        return await base.GetPolicyAsync(policyName);
    }
}
