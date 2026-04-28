// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public sealed class TenantName : BaseValueObject
{
    public string Value { get; private set; }

    public TenantName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 200)
            throw new ArgumentException("Tenant name must be 200 characters or fewer.", nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }
}
