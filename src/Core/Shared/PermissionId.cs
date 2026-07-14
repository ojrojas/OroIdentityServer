// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Shared;

/// <summary>
/// PermissionId is a value object that represents the unique identifier for a Permission entity. It encapsulates a Guid value and provides a factory method for creating instances.
/// </summary>
/// <param name="value">Identifier permission</param>
public sealed record PermissionId(Guid Value)
{
    public static PermissionId New(Guid? value) => new(value ?? Guid.CreateVersion7());
    public static PermissionId From(Guid value) => new(value);
}
