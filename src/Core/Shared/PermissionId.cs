// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Shared;

/// <summary>
/// PermissionId is a value object that represents the unique identifier for a Permission entity. It encapsulates a Guid value and provides a factory method for creating instances. The GetEquatibilityComponents method is overridden to ensure that equality comparisons are based on the Value property. This design promotes immutability and ensures that PermissionId instances can be compared based on their underlying Guid values.
/// </summary>
/// <param name="value">Identifier permission</param>
public sealed class PermissionId(Guid value) : BaseValueObject
{
    public Guid Value { get; private set; } = value;
    public static PermissionId New(Guid? value) => new(value ?? Guid.NewGuid());

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}
