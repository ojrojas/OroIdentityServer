// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class RoleClaimValue : BaseValueObject
{
    public string Value { get; private set; }

    public RoleClaimValue(string value) => Value = value;

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}