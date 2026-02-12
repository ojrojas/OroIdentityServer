// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class NameViewModel
{
   public string Value { get; set; }
    public NameViewModel(string value)
    {
        Value = value;
    }

    public static NameViewModel Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, "Name value is null or empty");
        return new NameViewModel(value);

    }
}
