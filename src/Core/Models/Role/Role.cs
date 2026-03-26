// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

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

    public void RemoveClaim(RoleClaim claim)
    {
        if (_claims.Remove(claim))
        {
            RaiseDomainEvent(new RoleClaimRemovedEvent(Id, claim.ClaimType, claim.ClaimValue));
        }
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

    public Role(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("RoleName cannot be null or empty.");

        Id = RoleId.New();
        Name = new RoleName(roleName);
        IsActive = true;
        Validate();
        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

    private Role() { }

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
