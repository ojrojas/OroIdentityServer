// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.DTOs;

public record TenantUserDto(
    Guid UserId,
    Guid RoleId,
    bool IsActive,
    DateTime JoinedAtUtc);
