// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.AspNetCore.Authorization;

namespace OroIdentityServer.Server.Authorization;

/// <summary>
/// Requires the authenticated principal to hold a <c>permission</c> claim whose value is
/// exactly <see cref="Permission"/>. Matching is exact: no wildcards or prefixes.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    /// <summary>The permission name (<c>Provider.Resource.Action</c>) that must be granted.</summary>
    public string Permission { get; } = permission;
}
