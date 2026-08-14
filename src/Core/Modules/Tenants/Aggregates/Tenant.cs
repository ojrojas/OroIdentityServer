// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.Aggregates;

public class Tenant : AggregateRoot<TenantId>, IAuditableEntity
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
        var setSlug = name.ToLower().Replace(" ", "-");
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

    public static Tenant From(Guid id, string name, string slug, bool isActive, DateTime createdAtUtc)
    {
        return new Tenant
        {
            Id = new TenantId(id),
            Name = new TenantName(name),
            Slug = new TenantSlug(slug),
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc
        };
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

    public TenantUser AddUser(UserId userId)
    {
        if (_tenantUsers.Any(tu => tu.UserId == userId))
            throw new InvalidOperationException("User is already a member of this tenant.");

        var tenantUser = new TenantUser(Id, userId);
        _tenantUsers.Add(tenantUser);

        RaiseDomainEvent(new TenantUserAddedEvent(Id, userId));

        return tenantUser;
    }
}
