// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace OroIdentity.Web.Client.Models;

public class RoleViewModel
{
    public IdViewModel RoleId { get; set; } = IdViewModel.Empty();

    public NameViewModel Name { get; set; } = NameViewModel.Create(string.Empty);

    public bool IsActive { get; set; }

    private readonly List<IdentificationTypeViewModel> _claims = new();
    public IReadOnlyCollection<IdentificationTypeViewModel> Claims => _claims.AsReadOnly();

    public void AddRoleClaimViewModel(IdentificationTypeViewModel roleClaim)
    {
        _claims.Add(roleClaim);
    }
}
