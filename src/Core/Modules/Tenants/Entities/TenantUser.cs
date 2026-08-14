// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Entities;

/// <summary>
/// Membership row that records "user U belongs to tenant T". The tenant only needs to know
/// WHICH users it contains; the user's permissions live on <c>UserRole</c> (the catalogue
/// of roles) and <c>User.TenantId</c> for home-tenant checks. Carrying a per-tenant role
/// here was duplicating the catalogue and caused the two systems to drift out of sync.
/// </summary>
public sealed class TenantUser : Entity<TenantUserId>, IAggregateRoot
{
    public TenantId TenantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    internal TenantUser(TenantId tenantId, UserId userId)
    {
        Id = TenantUserId.New();
        TenantId = tenantId;
        UserId = userId;
        IsActive = true;
        JoinedAtUtc = DateTime.UtcNow;
    }

    private TenantUser() { }
}
