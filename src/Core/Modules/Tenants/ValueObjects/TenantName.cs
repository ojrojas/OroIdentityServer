// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public sealed record TenantName(string Value)
{
    public static TenantName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 200)
            throw new ArgumentException("Tenant name must be 200 characters or fewer.", nameof(value));

        return new TenantName(value.Trim());
    }

    public override string ToString() => Value;
    public static implicit operator string(TenantName n) => n.Value;
}
