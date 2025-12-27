// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Tenant : BaseEntity<Tenant, TenantId>, IAuditableEntity, IAggregateRoot
{
    public TenantName? Name { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant(string name)
    {
        Id = TenantId.New();
        Name = new TenantName(name);
        IsActive = true;
        RaiseDomainEvent(new TenantCreateEvent(Id, Name));
    }

    private Tenant() { }

    public void Deactive()
    {
        if(!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new TenantDeactiveEvent(Id, Name));
    }

    public static Tenant CreateTenant(TenantName name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name?.Value);
        return new Tenant(name.Value);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name?.Value))
            throw new ArgumentException("Tenant name cannot be empty.");
    }
}

// Add repository interface for Tenant
public interface ITenantRepository
{
    Tenant? GetById(TenantId id);
    void Add(Tenant tenant);
    void Update(Tenant tenant);
    void Remove(Tenant tenant);
}
