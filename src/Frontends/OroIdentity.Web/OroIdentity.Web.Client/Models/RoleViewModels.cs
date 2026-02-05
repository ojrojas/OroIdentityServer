// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class RoleViewModel
{
    public Guid RoleId {get;set;}
    public required string Name {get;set; }
    public bool IsActive {get;set;}
    public IReadOnlyCollection<RoleClaimViewModel> Claims => _claims.AsReadOnly();
    private readonly IList<RoleClaimViewModel> _claims = [];

    public void AddRoleClaimViewModel(RoleClaimViewModel roleClaim)
    {
        _claims.Add(roleClaim);
    }
}
