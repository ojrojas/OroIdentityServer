// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Tenant : AggregateRoot<TenantId>, IAuditableEntity
{
    // Constructor vacío requerido por EF Core
    private Tenant() : base(null!)
    {
        // Constructor vacío para EF Core
        // Las propiedades se establecerán por reflexión
    }

    public string? Name { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant(TenantId tenantId, string name)
    : base(tenantId)
    {
        Id = new TenantId(Guid.CreateVersion7());
        Name = name;
        IsActive = true;
        RaiseDomainEvent(new TenantCreateEvent(tenantId, name));
    }

    public void Deactive()
    {
        if(!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new TenantDeactiveEvent(Id, Name));
    }

    public static Tenant CreateTenant(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Tenant(new TenantId(Guid.CreateVersion7()), name);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
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

public record TenantCreateEvent(TenantId TenantId, string Name): DomainEvent;
public record TenantDeactiveEvent(TenantId TenantId, string Name): DomainEvent;
