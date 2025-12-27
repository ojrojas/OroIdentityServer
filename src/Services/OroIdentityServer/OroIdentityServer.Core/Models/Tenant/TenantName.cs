// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class TenantName : BaseValueObject
{
    public string Value { get; private set; }
    public TenantName(string value)
    {
        Value = value;
    }

    public static TenantName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, "Tenant name value is null or empty");
        return new TenantName(value);

    }

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }

}