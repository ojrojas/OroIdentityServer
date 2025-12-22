// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public abstract class BaseEntity<TId>(TId Id) : IEquatable<BaseEntity<TId>>
{
    [Key]
    public TId Id { get; protected set; } = Id;

    public bool Equals(BaseEntity<TId>? obj)
    {
        return obj is BaseEntity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
       => HashCode.Combine(Id);

    public override bool Equals(object? obj) => Equals(obj as BaseEntity<TId>);
}