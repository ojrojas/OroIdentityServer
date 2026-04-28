// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public sealed class SubscriptionId(Guid value) : BaseValueObject
{
    public Guid Value { get; private set; } = value;

    public static SubscriptionId New() => new(Guid.CreateVersion7());
    public static SubscriptionId From(Guid value) => new(value);

    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }
}
