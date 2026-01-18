// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class RoleClaim
{
    private RoleClaim()
    {
        ClaimType = null!;
        ClaimValue = null!;
    }

    public RoleClaim(RoleClaimType claimType, RoleClaimValue claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
        IsActive = true;
        Id = Guid.CreateVersion7();
    }

    public RoleClaimType ClaimType { get; private set; }
    public RoleClaimValue ClaimValue { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid Id { get; private set; }

    // Add validation logic to RoleClaim
    public void Validate()
    {
        if (ClaimType == null || string.IsNullOrWhiteSpace(ClaimType.Value))
            throw new ArgumentException("Claim type cannot be empty.");
        if (ClaimValue == null || string.IsNullOrWhiteSpace(ClaimValue.Value))
            throw new ArgumentException("Claim value cannot be empty.");
    }

    public void UpdateClaim(RoleClaimType claimType, RoleClaimValue claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}