// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.ValueObjects;

public readonly record struct Timezone(string Value)
{
    public static Timezone From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Timezone(value);
    }

    public override string ToString() => Value;
}
