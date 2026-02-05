// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class RoleClaimViewModel
{
    public string ClaimType { get;  set; }
    public string ClaimValue { get;  set; }
    public bool IsActive { get; set; } = true;
    public Guid Id { get; set; }
}