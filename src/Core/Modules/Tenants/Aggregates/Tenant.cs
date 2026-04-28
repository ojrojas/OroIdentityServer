// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Aggregates;

public class TenantUser
{
    public  TenantUserId TenantUserId {get;set;}
    public TenantId? TenantId { get; private set; }
    public UserId? UserId { get; private set; }
    public bool IsOwner { get; private set; }
    public bool IsActive { get; private set; }
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public DateTime JoinedAtUtc { get; private set; }
}

public class Tenant : BaseEntity<Tenant, TenantId>, IAuditableEntity, IAggregateRoot
{
    public TenantName Name { get; private set; }
    public bool IsActive { get; private set; }
    public TenantSlug Slug { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<TenantUser> _tenantUsers = [];
    public IReadOnlyCollection<TenantUser> TenantUsers => _tenantUsers;

    public Tenant(string name) : base()
    {
        Id = TenantId.New();
        Name = new TenantName(name);
        IsActive = true;
        Slug = GenerateSlug(name);
        CreatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new TenantCreatedEvent(Id));
    }

    private static TenantSlug GenerateSlug(string name)
    {
        var setSlug =  name.ToLower().Replace(" ", "-");
        return new TenantSlug(setSlug);
    }

    private Tenant()
    {
        Name = null!;
        Slug = null!;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = false;
    }

    public static Tenant Create(string name)
    {
        var Tenant = new Tenant(name);
        Tenant.Validate();
        return Tenant;
    }


    public void Deactive()
    {
        if (!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new TenantSuspendedEvent(Id));
    }

    // Add validation logic to Tenant
    public void Validate()
    {
        if (Name == null || string.IsNullOrWhiteSpace(Name.Value))
            throw new ArgumentException("Identification type name cannot be empty.");
    }

    // Add method to update the name
    public void UpdateName(TenantName newName)
    {
        if (newName == null || string.IsNullOrWhiteSpace(newName.Value))
            throw new ArgumentException("New name cannot be null or empty.");

        if (Name != null && Name.Equals(newName)) return; // Avoid unnecessary updates

        Name = newName;
        RaiseDomainEvent(new TenantUpdatedEvent(Id, newName));
    }

    // Add method to activate the entity
    public void Activate()
    {
        if (IsActive) return; // Avoid unnecessary updates

        IsActive = true;
        RaiseDomainEvent(new TenantActivatedEvent(Id));
    }
}
