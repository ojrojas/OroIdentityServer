// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Entities;

public abstract class BaseValueObject<TClassProperty> where TClassProperty : struct, IEquatable<TClassProperty>
{
}

public abstract class BaseValueObject<TClassProperty, TProperty> where TClassProperty :
BaseValueObject<TClassProperty, TProperty>
{
    public TProperty Value { get; } = default!;

    public BaseValueObject(TProperty value)
    {
        if (string.IsNullOrEmpty(nameof(value)))
        {
            throw new ArgumentNullException(nameof(value), "NullOrEmpty");
        }

        Value = value;
    }

    public bool Equals(TClassProperty? other)
    {
        return other is not null && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is TClassProperty prop && Equals(prop);
    }

    public override int GetHashCode()
    {
        ArgumentNullException.ThrowIfNull(Value);
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }
}
