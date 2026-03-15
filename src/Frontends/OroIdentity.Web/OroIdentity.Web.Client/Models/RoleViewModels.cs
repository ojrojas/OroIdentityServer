// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace OroIdentity.Web.Client.Models;

public class RoleViewModel
{
    public IdViewModel Id { get; set; } = IdViewModel.Empty();

    public NameViewModel Name { get; set; } = NameViewModel.Create(string.Empty);

    public bool IsActive { get; set; }

    public List<RoleClaimViewModel> Claims { get; set; } = [];

    public void AddRoleClaimViewModel(RoleClaimViewModel roleClaim)
    {
        Claims.Add(roleClaim);
    }
}
