// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed record IdentificationTypeName(string Value) :
    BaseValueObject
{
    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }

    public static IdentificationTypeName Create(string Value)
    {
        if(string.IsNullOrWhiteSpace(Value)) throw new ArgumentNullException("Email is required");
        return new IdentificationTypeName(Value);
    }
}