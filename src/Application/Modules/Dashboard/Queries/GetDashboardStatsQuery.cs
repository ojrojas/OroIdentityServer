// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Modules.Dashboard.DTOs;

namespace OroIdentityServer.Application.Modules.Dashboard.Queries;

public record GetDashboardStatsQuery(Guid? TenantId = null) : IQuery<DashboardStatsDto>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
