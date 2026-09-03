// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Roles.Aggregates;

public sealed class Role : AggregateRoot<RoleId>, IAuditableEntity
{
    private readonly IList<RolePermission> _rolePermissions = [];

    public bool IsActive { get; private set; }
    public RoleName Name { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int Level { get; private set; } = 10;
    public RoleId? ParentRoleId { get; private set; }
    public Role? ParentRole { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role()
    {
        Name = null!;
    }

    public Role(RoleName name, int level = 10, RoleId? parentRoleId = null)
    {
        Id = RoleId.New();
        Name = name ?? throw new Exception("role.name.required Role name is required");
        IsActive = true;
        SetLevel(level);
        ParentRoleId = parentRoleId;

        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

    public void SetLevel(int level)
    {
        if (level < 10 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be between 10 and 100");
        Level = level;
    }

    public void SetParentRole(RoleId? parentRoleId)
    {
        if (parentRoleId != null && parentRoleId.Value == Id.Value)
            throw new InvalidOperationException("Role cannot be its own parent");
        ParentRoleId = parentRoleId;
    }

    public void AddPermission(Permission permission)
    {
        if (!IsActive)
            throw new Exception("role.inactive Cannot modify an inactive role");

        if (_rolePermissions.Any(p => p.PermissionId == permission.Id))
            throw new Exception("role.permission.duplicate Permission already assigned");

        _rolePermissions.Add(new RolePermission(Id, permission.Id));
        RaiseDomainEvent(new RolePermissionAddedEvent(Id, permission.Id));
    }

    public void RemovePermission(PermissionId permissionId)
    {
        if (!IsActive)
            throw new Exception("role.inactive Cannot modify an inactive role");

        var existing = _rolePermissions
            .FirstOrDefault(p => p.PermissionId == permissionId)
            ?? throw new Exception("role.permission.not_found Permission not found in role");

        _rolePermissions.Remove(existing);
        RaiseDomainEvent(new RolePermissionRemovedEvent(Id, permissionId));
    }

    public void UpdateName(RoleName newName)
    {
        if (newName == null)
            throw new Exception("role.name.require Role name is required");

        if (Name.Equals(newName)) return;

        Name = newName;
        RaiseDomainEvent(new RoleUpdatedEvent(Id, newName));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new Exception("role.already_inactive Role is already inactive");

        IsActive = false;
        RaiseDomainEvent(new RoleDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new Exception("role.already_active Role is already active");

        IsActive = true;
        RaiseDomainEvent(new RoleActivatedEvent(Id));
    }
}