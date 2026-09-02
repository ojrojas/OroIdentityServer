// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using Microsoft.AspNetCore.Authorization;

namespace OroIdentityServer.Server.Authorization;

public sealed class HierarchyRequirement : IAuthorizationRequirement
{
    public string? TargetUserIdClaimType { get; }
    public string? RelationshipType { get; }

    public HierarchyRequirement(string? targetUserIdClaimType = null, string? relationshipType = null)
    {
        TargetUserIdClaimType = targetUserIdClaimType;
        RelationshipType = relationshipType;
    }
}
