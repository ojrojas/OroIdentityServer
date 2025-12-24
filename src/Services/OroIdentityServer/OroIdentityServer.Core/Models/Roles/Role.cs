// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Role : AggregateRoot<RoleId>, IAuditableEntity
{
    private readonly IList<RoleClaim> _claims = [];

    private Role() : base(null!)
    {
    }

    public Role(RoleId id, RoleName roleName) : base(id)
    {
        IsActive = true;
        Name = roleName;
        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

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

    // Add Create method
    public static Role Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("RoleName cannot be null or empty.");

        return new Role(new RoleId(Guid.NewGuid()), new RoleName(roleName));
    }

    public static Role Create(RoleName roleName)
    {
        if (roleName == null || string.IsNullOrWhiteSpace(roleName.Value))
            throw new ArgumentException("RoleName cannot be null or empty.");

        return new Role(new RoleId(Guid.NewGuid()), roleName);
    }
}

public sealed record RoleUpdatedEvent(RoleId RoleId, RoleName NewName) : DomainEvent;
public sealed record RoleClaimAddedEvent(RoleId RoleId, RoleClaimType ClaimType, RoleClaimValue ClaimValue) : DomainEvent;
public sealed record RoleCreateEvent(RoleId RoleId) : DomainEvent;
public sealed record RoleDeactiveEvent(RoleId RoleId) : DomainEvent;
public sealed record RoleDeletedEvent(RoleId RoleId) : DomainEvent;
