// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Queries;

public record GetValidationLogDailySummaryResponse(IReadOnlyList<DailyValidationCount> Days);

public record DailyValidationCount(DateOnly Date, int SucceededCount, int FailedCount);
