// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Entities;

public sealed class TenantUser : BaseEntity<TenantUser, TenantUserId>
{
    public TenantId TenantId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    internal TenantUser(TenantId tenantId, UserId userId, string role)
    {
        Id = TenantUserId.New();
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        IsActive = true;
        JoinedAtUtc = DateTime.UtcNow;
    }

    private TenantUser() { }
}
