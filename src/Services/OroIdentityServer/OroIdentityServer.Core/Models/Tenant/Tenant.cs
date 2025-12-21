// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Tenant : AggregateRoot<TenantId>, IAuditableEntity
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant()
    {
        Id = new TenantId(Guid.Empty);
        Name = string.Empty;
    }
    private Tenant(TenantId tenantId, string name)
    {
        Name = name;
        IsActive = true;

        RaiseDomainEvent(new TenantCreateEvent(tenantId, name));
    }

    public static Tenant CreateTenant(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Tenant(new TenantId(Guid.CreateVersion7()), name);
    }
}

public record TenantCreateEvent(TenantId TenantId, string Name): DomainEvent;
