// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class RoleViewModel
{
    public IdViewModel RoleId {get;set;}
    public NameViewModel Name {get;set; }
    public bool IsActive {get;set;}
    public IReadOnlyCollection<IdentificationTypeViewModel> Claims => _claims.AsReadOnly();
    private readonly IList<IdentificationTypeViewModel> _claims = [];

    public void AddRoleClaimViewModel(IdentificationTypeViewModel roleClaim)
    {
        _claims.Add(roleClaim);
    }
}
