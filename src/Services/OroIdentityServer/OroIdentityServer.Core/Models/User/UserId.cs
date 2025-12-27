// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed class UserId : BaseValueObject
{
    public Guid Value { get; private set; }

    public UserId(Guid value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.CreateVersion7());

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}