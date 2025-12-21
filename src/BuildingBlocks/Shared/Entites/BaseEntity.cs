// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
{
    [Key]
    public TId Id { get; protected set; } = default!;

    // public TId CreatedBy { get; protected set; }
    // public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now.ToUniversalTime();
    // public TId? ModifiedBy { get; protected set; }
    // public DateTimeOffset? ModifiedOn { get; set; }
    // public EntityBaseState State { get; set; } = EntityBaseState.ACTIVE;

    public bool Equals(BaseEntity<TId>? obj)
    {
        return obj is BaseEntity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
       => HashCode.Combine(Id);

    public override bool Equals(object obj) => Equals(obj as BaseEntity<TId>);
}