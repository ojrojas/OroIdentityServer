// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.DTOs;

public sealed record RoleDto
{
    public Guid Id { get; set; }
    private readonly IList<RoleClaimDto> _claims = [];
    public bool IsActive { get; set; }
    public RoleName? Name { get; set; }
    public IReadOnlyCollection<RoleClaimDto> Claims => _claims.AsReadOnly();
}
