// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class SecurityUser : BaseEntity<Guid>, IAuditableEntity
{
    public required string PasswordHash { get; set; }
    public string SecurityStamp { get; set; } = string.Empty;
    public Guid ConcurrencyStamp { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; } = 0;

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
