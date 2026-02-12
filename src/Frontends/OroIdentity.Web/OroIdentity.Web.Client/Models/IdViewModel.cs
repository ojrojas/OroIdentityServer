// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class IdViewModel
{
    public Guid Value { get; set; }

    public IdViewModel(Guid value) => Value = value;

    public static IdViewModel New() => new(Guid.CreateVersion7());
    public static IdViewModel Empty() => new(Guid.Empty);

}