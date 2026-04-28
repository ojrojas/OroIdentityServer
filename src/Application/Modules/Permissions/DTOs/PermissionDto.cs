// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.DTOs;

public sealed record PermissionDto(
    Guid PermissionId, 
    string Name,
    string DisplayName,
    string? Description,
    string Resource,
    bool IsSystem);