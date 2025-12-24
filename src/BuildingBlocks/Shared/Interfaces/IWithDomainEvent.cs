// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Interfaces;

/// <summary>
/// Entities that contain domain events
/// </summary>
public interface IWithDomainEvents
{
    /// <summary>
    /// Domain events associated with the entity
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    /// <summary>
    /// Clears all domain events associated with the entity
    /// </summary>
    void ClearDomainEvents();
}