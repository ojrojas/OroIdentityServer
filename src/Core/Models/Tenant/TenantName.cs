// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class TenantName : BaseValueObject
{
    public string Value { get; private set; }

    public TenantName(string value) => Value = value;

    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }

    public static TenantName Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("Name is required");
        return new TenantName(value);
    }

    public static TenantName Empty => new(string.Empty);
}