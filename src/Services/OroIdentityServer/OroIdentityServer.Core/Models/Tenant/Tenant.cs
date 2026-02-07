// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class Tenant :
BaseEntity<Tenant, TenantId>, IAuditableEntity, IAggregateRoot
{
    public Tenant(string name) : base()
    {
        Id = TenantId.New();
        Name = new TenantName(name);
        IsActive = true;
        RaiseDomainEvent(new TenantCreateEvent(Id));
    }

    private Tenant()
    {
        Name = null!;
    }

    public static Tenant Create(string name)
    {

        var Tenant = new Tenant(name);
        Tenant.Validate();
        return Tenant;
    }

    public TenantName Name { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactive()
    {
        if (!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new TenantDeactiveEvent(Id));
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

    // Update Deactive method to ensure consistency
    public void Deactivate()
    {
        if (!IsActive) return; // Avoid unnecessary updates

        IsActive = false;
        RaiseDomainEvent(new TenantDeactivatedEvent(Id));
    }
}
