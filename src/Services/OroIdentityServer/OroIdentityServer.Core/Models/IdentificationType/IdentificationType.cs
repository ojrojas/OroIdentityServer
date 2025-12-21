// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class IdentificationType : AggregateRoot<IdentificationTypeId>, IAuditableEntity
{
    public IdentificationType(IdentificationTypeId id, IdentificationTypeName name) : base(id)
    {
        Name = name;
        IsActive = true;
        RaiseDomainEvent(new IdentificationTypeCreateEvent(Id));
    }

    public IdentificationTypeName Name { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactive()
    {
        if(!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new IdentificationTypeDeactiveEvent(Id));
    }
}

public sealed record IdentificationTypeCreateEvent(IdentificationTypeId IdentificationTypeId) : DomainEvent;
public sealed record IdentificationTypeDeactiveEvent(IdentificationTypeId IdentificationTypeId) : DomainEvent;
