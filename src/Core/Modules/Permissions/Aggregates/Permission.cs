// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Permissions.Aggregates;

/// <summary>
/// Represents a permission within the system, defining access rights to resources and actions
/// </summary>
public sealed class Permission : BaseEntity<Permission, PermissionId>, IAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string? Description { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    /// <summary>
    /// Indicates whether this permission is a system-level that cannot be modified or deleted
    /// </summary>
    public bool IsSystem { get; private set; }

    private Permission()
    {
        Id = null!;
    }

    public Permission(PermissionId? id, string provider, string? description, string action, string resource, bool isSystem)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Error Action", "Action cannot be empty.");
        Id = id ?? PermissionId.New(null);
        Provider = provider;
        Description = description;
        Action = action;
        Resource = resource;
        IsSystem = isSystem;
        NormalizeName();

        RaiseDomainEvent(new PermissionCreatedEvent(Id));
    }

    public static Permission Create(
        string provider,
        string? description,
        string action,
        string resource,
        bool isSystem)
    {
        return new Permission(PermissionId.New(null), provider, description, action, resource, isSystem);
    }

    public void Update(string provider, string? description, string action, string resource, bool isSystem)
    {
        if (IsSystem)
            throw new InvalidOperationException("System permissions cannot be modified.");

        Provider = NormalizeProperty(provider);
        Resource = NormalizeProperty(resource);
        Action = NormalizeProperty(action);
        Description = description;
        IsSystem = isSystem;

        NormalizeName();

        RaiseDomainEvent(new PermissionUpdatedEvent(Id));
    }

    public void Delete()
    {
        if (IsSystem) throw new InvalidOperationException("System permissions cannot be deleted.");

        RaiseDomainEvent(new PermissionDeletedEvent(Id));
    }

    private static string NormalizeProperty(string value) => value.Trim().ToLowerInvariant();

    private void NormalizeName()
    {
        Name = $"{Provider}.{Resource}.{Action}".ToLowerInvariant();
    }
}