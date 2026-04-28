// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public sealed partial class TenantSlug : BaseValueObject
{
    public string Value { get; private set; }

    public TenantSlug(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 50)
            throw new ArgumentException("Slug must be 50 characters or fewer.", nameof(value));

        if (!SlugPattern().IsMatch(value))
            throw new ArgumentException("Slug must contain only lowercase alphanumeric characters and hyphens.", nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}

