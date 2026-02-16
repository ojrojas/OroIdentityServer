// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class IdViewModel(Guid value)
{
    public Guid Value { get; set; } = value;

    public static IdViewModel New(Guid id) => new(id);
    public static IdViewModel Empty() => new(Guid.Empty);

}