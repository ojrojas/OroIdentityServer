// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public class RoleClaimId : BaseValueObject
{
    public Guid Value { get; private set; }

    public RoleClaimId(Guid value) => Value = value;

    public static RoleClaimId New() => new(Guid.CreateVersion7());
    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }

    public static bool TryParse(string? input, IFormatProvider? formatProvider, out RoleClaimId result)
    {
        if (Guid.TryParse(input, out var guid))
        {
            result = new RoleClaimId(guid);
            return true;
        }
        result = null!;
        return false;
    }
}