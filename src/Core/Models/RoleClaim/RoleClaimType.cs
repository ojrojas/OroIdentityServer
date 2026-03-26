// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class RoleClaimType : BaseValueObject
{
    public string Value { get; private set; }

    public RoleClaimType(string value) => Value = value;

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}