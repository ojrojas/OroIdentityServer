// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Permissions.Aggregates;

public sealed class Permission : BaseEntity<Permission, PermissionId>, IAuditableEntity, IAggregateRoot
{
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }

    private Permission()
    {
        TenantId = default!;
        Name = string.Empty;
        DisplayName = string.Empty;
        Resource = string.Empty;
    }

    public Permission(PermissionId? id, TenantId tenantId, string name, string displayName, string? description, string resource, bool isSystem)
    {
        Id = id ?? PermissionId.New(null);
        TenantId = tenantId;
        Name = name;
        DisplayName = displayName;
        Description = description;
        Resource = resource;
        IsSystem = isSystem;

        RaiseDomainEvent(new PermissionCreatedEvent(TenantId, Id));
    }

    public static Permission Create(PermissionId? id, TenantId tenantId, string name, string displayName, string? description, string resource, bool isSystem)
    {
        return new Permission(id, tenantId, name, displayName, description, resource, isSystem);
    }

    public void Update(string name, string displayName, string? description, string resource)
    {
        if (IsSystem)
            throw new InvalidOperationException("System permissions cannot be modified.");

        Name = name;
        DisplayName = displayName;
        Description = description;
        Resource = resource;

        RaiseDomainEvent(new PermissionUpdatedEvent(TenantId, Id));
    }
}
