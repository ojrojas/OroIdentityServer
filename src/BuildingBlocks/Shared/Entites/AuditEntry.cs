// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public class AuditEntry
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = string.Empty;

    public string? ChangesJson { get; set; }

    public ICollection<AuditEntryProperty> Properties { get; set; } = new List<AuditEntryProperty>();

    [NotMapped]
    public List<PropertyChange> TemporaryProperties { get; set; } = new List<PropertyChange>();
    
    [NotMapped]
    public EntityState State { get; set; }
}

public class AuditEntryProperty
{
    public int Id { get; set; }
    public int AuditEntryId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public AuditEntry AuditEntry { get; set; } = null!;
}

public class PropertyChange
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}