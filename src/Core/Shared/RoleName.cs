// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Shared;

public sealed record RoleName(string Value)
{
    public static RoleName Empty => new(string.Empty);

    public static RoleName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Role name cannot be null or empty.", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Role name cannot exceed 100 characters.", nameof(value));

        return new RoleName(value.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(RoleName r) => r.Value;
}
