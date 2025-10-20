// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public abstract class BaseEntity<TId> where TId : struct, IEquatable<TId>
{
    [Key]
    public TId Id { get; set; }
    public TId CreatedBy { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now.ToUniversalTime();
    public TId? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public EntityBaseState State { get; set; } = EntityBaseState.ACTIVE;
}

public abstract class BaseEntity<T, TId> where T : BaseEntity<T, TId>
{
    public TId Id { get; set; } = default!;
}
