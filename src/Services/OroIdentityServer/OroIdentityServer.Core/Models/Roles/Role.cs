// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class Role : AggregateRoot<RoleId>, IAuditableEntity
{
    private readonly IList<RoleClaim> _claims = [];


    public Role(RoleId id, RoleName roleName) : base(id)
    {
        IsActive = true;
        Name = roleName;
        RaiseDomainEvent(new RoleCreateEvent(Id));
    }

    public bool IsActive { get; private set; }
    public RoleName Name { get; private set; }

    public IReadOnlyCollection<RoleClaim> Claims => _claims.AsReadOnly();

    public void AddClaim(RoleClaim claim)
    {
        _claims.Add(claim);
        RaiseDomainEvent(new RoleClaimAddedEvent(Id, claim.ClaimType, claim.ClaimValue));
    }
}

public sealed record RoleClaimAddedEvent(RoleId RoleId, RoleClaimType ClaimType, RoleClaimValue ClaimValue) : DomainEvent;

public sealed record RoleCreateEvent(RoleId RoleId) : DomainEvent;
public sealed record RoleDeactiveEvent(RoleId RoleId) : DomainEvent;
