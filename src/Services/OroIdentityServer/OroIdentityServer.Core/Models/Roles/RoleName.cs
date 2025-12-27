// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class RoleName : BaseValueObject
{
    public string Value { get; private set; }
    public RoleName(string value)
    {
        Value = value;
    }

    public static RoleName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, "Role name value is null or empty");
        return new RoleName(value);

    }

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }

}