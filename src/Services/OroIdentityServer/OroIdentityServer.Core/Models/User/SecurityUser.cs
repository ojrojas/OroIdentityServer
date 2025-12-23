// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class SecurityUser : AggregateRoot<SecurityUserId>, IAuditableEntity 
{
    public SecurityUser(SecurityUserId id, string passwordHash, string securityStamp, Guid concurrencyStamp) : base(id)
    {
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        ConcurrencyStamp = concurrencyStamp;

        RaiseDomainEvent(new SecurityUserCreatedEvent(id));
    }

    public string PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; } = string.Empty;
    public Guid ConcurrencyStamp { get; private set; }
    public DateTime? LockoutEnd { get; private set; } = DateTime.UtcNow;
    public bool LockoutEnabled { get; private set; } = false;
    public int AccessFailedCount { get; private set; } = 0;

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        RaiseDomainEvent(new AccessFailedIncrementedEvent(Id, AccessFailedCount));
    }

    // Override ResetAccessFailedCount to raise event
    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        RaiseDomainEvent(new AccessFailedResetEvent(Id));
    }

    // Override LockUntil to raise event
    public void LockUntil(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        RaiseDomainEvent(new UserLockedEvent(Id, lockoutEnd));
    }

    // Add method to set LockoutEnabled with an event
    public void SetLockoutEnabled(bool enabled)
    {
        if (LockoutEnabled == enabled) return; // Avoid unnecessary updates

        LockoutEnabled = enabled;
        RaiseDomainEvent(new LockoutEnabledChangedEvent(Id, enabled));
    }

    // Add Create method
    public static SecurityUser Create(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be null or empty.");

        return new SecurityUser(
            new SecurityUserId(Guid.NewGuid()),
            passwordHash,
            Guid.NewGuid().ToString(),
            Guid.NewGuid()
        );
    }

    // Add domain events
    public sealed record SecurityUserCreatedEvent(SecurityUserId SecurityUserId) : DomainEvent;
    public sealed record AccessFailedIncrementedEvent(SecurityUserId SecurityUserId, int FailedCount) : DomainEvent;
    public sealed record AccessFailedResetEvent(SecurityUserId SecurityUserId) : DomainEvent;
    public sealed record UserLockedEvent(SecurityUserId SecurityUserId, DateTime LockoutEnd) : DomainEvent;
    public sealed record LockoutEnabledChangedEvent(SecurityUserId SecurityUserId, bool Enabled) : DomainEvent;
}
