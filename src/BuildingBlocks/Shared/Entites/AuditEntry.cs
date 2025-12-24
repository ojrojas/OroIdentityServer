// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

/// <summary>
/// Represents an audit entry for tracking changes to entities.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the entity being audited.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the identifier of the entity being audited.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the action performed (e.g., Create, Update, Delete).
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the timestamp of when the action occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who performed the action.
    /// </summary>
    public Guid UserId { get; set; } = Guid.Empty;
    /// <summary>
    /// Gets or sets the username of the user who performed the action.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the JSON representation of the changes made.
    /// </summary>
    public string? ChangesJson { get; set; }
    /// <summary>
    /// Gets or sets the collection of property changes associated with this audit entry.
    /// </summary>
    public ICollection<AuditEntryProperty> Properties { get; set; } = new List<AuditEntryProperty>();

    /// <summary>
    /// Gets or sets the temporary properties that are not mapped to the database.
    /// </summary>
    [NotMapped]
    public List<PropertyChange> TemporaryProperties { get; set; } = new List<PropertyChange>();
    /// <summary>
    /// Gets or sets a value indicating whether there are any temporary properties.
    /// </summary>
    [NotMapped]
    public EntityState State { get; set; }
}

/// <summary>
/// Represents a property change within an audit entry.
/// </summary>
public class AuditEntryProperty
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit entry property.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the associated audit entry.
    /// </summary>
    public int AuditEntryId { get; set; }
    /// <summary>
    /// Gets or sets the name of the property that was changed.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the old value of the property before the change.
    /// </summary>
    public string? OldValue { get; set; }
    /// <summary>
    /// Gets or sets the new value of the property after the change.
    /// </summary>
    public string? NewValue { get; set; }
    /// <summary>
    /// Gets or sets the associated audit entry.
    /// </summary>
    public AuditEntry AuditEntry { get; set; } = null!;
}

/// <summary>
/// Represents a temporary property change used during auditing.
/// </summary>
public class PropertyChange
{
    /// <summary>
    /// Gets or sets the name of the property that was changed.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the old value of the property before the change.
    /// </summary>
    public string? OldValue { get; set; }
    /// <summary>
    /// Gets or sets the new value of the property after the change.
    /// </summary>
    public string? NewValue { get; set; }
}