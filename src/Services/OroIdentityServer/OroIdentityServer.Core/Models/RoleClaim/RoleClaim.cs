// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class RoleClaim : BaseEntity<Guid>, IAuditableEntity, IAggregateRoot
{
    public Role? Role { get; set; }
    public Guid RoleId { get; set; }
    public required string ClaimType { get; set; }
    public required string ClaimValue { get; set; }
}