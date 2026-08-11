// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Dashboard.DTOs;

public sealed record DashboardStatsDto(
    int UsersCreatedToday,
    int RolesCreatedToday,
    int TenantsCreatedToday,
    int IdentificationTypesCreatedToday,
    int ConnectedUsers,
    IReadOnlyList<RecentEntityDto> RecentlyCreated);
