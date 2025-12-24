// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Role : BaseEntity<Role, RoleId>, IAuditableEntity, IAggregateRoot
{
    private readonly IList<RoleClaim> _claims = [];

    public bool IsActive { get; private set; }
    public RoleName? Name { get; private set; }

    public IReadOnlyCollection<RoleClaim> Claims => _claims.AsReadOnly();

    public void AddClaim(RoleClaim claim)
    {
        _claims.Add(claim);
        RaiseDomainEvent(new RoleClaimAddedEvent(Id, claim.ClaimType, claim.ClaimValue));
    }

    public void UpdateName(RoleName newName)
    {
        if (newName == null || string.IsNullOrWhiteSpace(newName.Value))
            throw new ArgumentException("New name cannot be null or empty.");

        if (Name != null && Name.Equals(newName)) return; // Avoid unnecessary updates

        Name = newName;
        RaiseDomainEvent(new RoleUpdatedEvent(Id, newName));
    }

    public void Validate()
    {
        if (Name == null || string.IsNullOrWhiteSpace(Name.Value))
            throw new ArgumentException("Role name cannot be empty.");
    }

    public Role(RoleName roleName)
    {
        if (roleName == null || string.IsNullOrWhiteSpace(roleName.Value))
            throw new ArgumentException("RoleName cannot be null or empty.");

        Id = new RoleId(Guid.CreateVersion7());
        Name = roleName;
        IsActive = true;
        Validate();
        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

    public void Deactive()
    {
        if(!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new RoleDeactiveEvent(Id));
    }

    public void Delete()
    {
        RaiseDomainEvent(new RoleDeletedEvent(Id));
    }
}
