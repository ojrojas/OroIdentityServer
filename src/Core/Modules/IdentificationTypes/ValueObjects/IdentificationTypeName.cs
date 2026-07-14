// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.IdentificationTypes.ValueObjects;

public sealed record IdentificationTypeName(string Value)
{
    public static IdentificationTypeName Empty => new(string.Empty);

    public static IdentificationTypeName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name is required.", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Identification type name cannot exceed 100 characters.", nameof(value));

        return new IdentificationTypeName(value.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(IdentificationTypeName n) => n.Value;
}
