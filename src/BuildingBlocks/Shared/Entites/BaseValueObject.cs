// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public abstract record BaseValueObject : IEquatable<BaseValueObject>
{
    protected abstract IEnumerable<object?> GetEquatibilityComponents();

    public override int GetHashCode()
    {
        return GetEquatibilityComponents().Aggregate(1, HashCode.Combine);
    }
}
