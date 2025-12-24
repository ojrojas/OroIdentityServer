// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

/// <summary>
/// Base entity with a Guid identifier
/// </summary>
public abstract class BaseEntity : WithDomainEventBase
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();
}

/// <summary>
/// Base entity with a typed identifier
/// </summary>
/// <typeparam name="TId">Tid Identifier</typeparam>
public abstract class BaseEntity<TId> : WithDomainEventBase where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public TId Id { get; set; } = default!;
}

/// <summary>
/// Base entity with a typed identifier and self-referencing generic type
/// </summary>
/// <typeparam name="T">T entity type</typeparam>
/// <typeparam name="TId">TId identifier</typeparam>
public abstract class BaseEntity<T, TId> : WithDomainEventBase where T : BaseEntity<T, TId>
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public TId Id { get; set; } = default!;
}
