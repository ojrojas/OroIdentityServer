// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Roles.Aggregates;

public sealed class Role : BaseEntity<Role, RoleId>, IAggregateRoot
{
    private readonly IList<RolePermission> _rolePermissions = [];

    public bool IsActive { get; private set; }
    public RoleName Name { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public Role(RoleName name)
    {
        Id = RoleId.New();
        Name = name ?? throw new DomainException("role.name.required", "Role name is required");
        IsActive = true;

        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

    public void AddPermission(Permission permission)
    {
        if (!IsActive)
            throw new DomainException("role.inactive", "Cannot modify an inactive role");

        if (_rolePermissions.Any(p => p.PermissionId == permission.Id))
            throw new DomainException("role.permission.duplicate", "Permission already assigned");

        _rolePermissions.Add(new RolePermission(Id, permission.Id));
        RaiseDomainEvent(new RolePermissionAddedEvent(Id, permission.Id));
    }

    public void RemovePermission(PermissionId permissionId)
    {
        if (!IsActive)
            throw new DomainException("role.inactive", "Cannot modify an inactive role");

        var existing = _rolePermissions
            .FirstOrDefault(p => p.PermissionId == permissionId)
            ?? throw new DomainException("role.permission.not_found", "Permission not found in role");

        _rolePermissions.Remove(existing);
        RaiseDomainEvent(new RolePermissionRemovedEvent(Id, permissionId));
    }

    public void UpdateName(RoleName newName)
    {
        if (newName == null)
            throw new DomainException("role.name.required", "Role name is required");

        if (Name.Equals(newName)) return;

        Name = newName;
        RaiseDomainEvent(new RoleUpdatedEvent(Id, newName));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("role.already_inactive", "Role is already inactive");

        IsActive = false;
        RaiseDomainEvent(new RoleDeactivatedEvent(Id));
    }
}