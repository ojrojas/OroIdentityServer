// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.RoleClaims.Entities;

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

    private RoleClaim(Guid id, RoleClaimType claimType, RoleClaimValue roleClaimValue, bool isActive)
    {
        Id = id;
        ClaimType = claimType;
        ClaimValue = roleClaimValue;
        IsActive = isActive;
    }

    public static RoleClaim From(Guid id, string claimType, string claimValue, bool isActive)
    {
        return new RoleClaim(id, new RoleClaimType(claimType), new RoleClaimValue(claimValue), isActive);
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