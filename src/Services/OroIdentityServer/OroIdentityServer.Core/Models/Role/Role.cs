// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class Role : BaseEntity<Guid>, IAuditableEntity, IAggregateRoot
{
    private Role()
    {
        RoleName = default!;
        NormalizedName = string.Empty;
    }

    public Role(RoleName RoleName)
    {
        this.RoleName = RoleName;
        NormalizedName = RoleName.Value;
    }

    public RoleName RoleName { get; private set; }
    public string NormalizedName { get; init; }
    public Guid ConcurrencyStamp { get; set; }
}