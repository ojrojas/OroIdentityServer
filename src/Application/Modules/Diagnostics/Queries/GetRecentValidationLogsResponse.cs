// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Queries;

public record GetRecentValidationLogsResponse(IReadOnlyList<ValidationLogEntry> Entries);

public record ValidationLogEntry(
    DateTime OccurredAtUtc,
    string EventType,
    bool Succeeded,
    Guid? UserId,
    string? ClientId,
    string? Scopes,
    string? FailureReason);
