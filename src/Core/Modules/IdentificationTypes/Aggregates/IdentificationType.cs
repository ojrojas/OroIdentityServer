// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;

public class IdentificationType :
BaseEntity<IdentificationType, IdentificationTypeId>, IAuditableEntity, IAggregateRoot
{
    public IdentificationTypeName Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    
    public IdentificationType(string name) : base()
    {
        Id = IdentificationTypeId.New(null);
        Name = new IdentificationTypeName(name);
        IsActive = true;
        RaiseDomainEvent(new IdentificationTypeCreateEvent(Id));
    }

    private IdentificationType()
    {
        Id = null!;
        Name = null!;
        CreatedAtUtc = new DateTime();
    }

    public static IdentificationType Create(string name)
    {
        var identificationType = new IdentificationType(name);
        identificationType.Validate();
        return identificationType;
    }

    // Add validation logic to IdentificationType
    public void Validate()
    {
        if (Name == null || string.IsNullOrWhiteSpace(Name.Value))
            throw new ArgumentException("Identification type name cannot be empty.");
    }

    // Add method to update the name
    public void UpdateName(IdentificationTypeName newName)
    {
        if (newName == null || string.IsNullOrWhiteSpace(newName.Value))
            throw new ArgumentException("New name cannot be null or empty.");

        if (Name != null && Name.Equals(newName)) return; // Avoid unnecessary updates

        Name = newName;
        RaiseDomainEvent(new IdentificationTypeUpdatedEvent(Id, newName));
    }

    // Add method to activate the entity
    public void Activate()
    {
        if (IsActive) return; // Avoid unnecessary updates

        IsActive = true;
        RaiseDomainEvent(new IdentificationTypeActivatedEvent(Id));
    }

    // Update Deactive method to ensure consistency
    public void Deactivate()
    {
        if (!IsActive) return; // Avoid unnecessary updates

        IsActive = false;
        RaiseDomainEvent(new IdentificationTypeDeactivatedEvent(Id));
    }
}
