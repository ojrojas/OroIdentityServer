// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

/// <summary>
/// Base class for entities with domain events
/// </summary>
public abstract class WithDomainEventBase: IWithDomainEvents
{
    /// <summary>
    /// Domain events associated with the entity
    /// </summary>
    private readonly IList<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events associated with the entity
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    /// <summary>
    /// Raises a domain event
    /// </summary>
    /// <param name="domainEvent">Domain event to raise</param>
    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    /// <summary>
    /// Clears all domain events associated with the entity
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}