// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class RoleClaim
{
    public RoleClaim(RoleClaimType claimType, RoleClaimValue claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }

    public RoleClaimType ClaimType { get; }
    public RoleClaimValue ClaimValue { get; }
    public bool IsActive { get; private set; } = true;

    // Add validation logic to RoleClaim
    public void Validate()
    {
        if (ClaimType == null || string.IsNullOrWhiteSpace(ClaimType.Value))
            throw new ArgumentException("Claim type cannot be empty.");
        if (ClaimValue == null || string.IsNullOrWhiteSpace(ClaimValue.Value))
            throw new ArgumentException("Claim value cannot be empty.");
    }
}