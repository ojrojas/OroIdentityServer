// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

/// <summary>
/// Base class for value objects
/// </summary>
public abstract record BaseValueObject : IEquatable<BaseValueObject>
{
    /// <summary>
    /// Gets the components that define the equality of the value object
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<object?> GetEquatibilityComponents();

    /// <summary>
    /// Checks equality between two value objects
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return GetEquatibilityComponents().Aggregate(1, HashCode.Combine);
    }
}
