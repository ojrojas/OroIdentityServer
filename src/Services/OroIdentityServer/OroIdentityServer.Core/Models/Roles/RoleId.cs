// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class RoleId : BaseValueObject
{
    public Guid Value { get; private set; }

    public RoleId(Guid value) => Value = value;

    public static RoleId New() => new(Guid.CreateVersion7());
    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }
}