// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class SecurityUser : AggregateRoot<Guid>, IAuditableEntity 
{
    public SecurityUser(Guid id, string passwordHash, string securityStamp, Guid concurrencyStamp, DateTime? lockoutEnd, bool lockoutEnabled, int accessFailedCount) : base(id)
    {
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        ConcurrencyStamp = concurrencyStamp;
        LockoutEnd = lockoutEnd;
        LockoutEnabled = lockoutEnabled;
        AccessFailedCount = accessFailedCount;
    }

    public string PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; } = string.Empty;
    public Guid ConcurrencyStamp { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; } = true;
    public int AccessFailedCount { get; private set; } = 0;

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
    }

    public void LockUntil(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
    }
}
