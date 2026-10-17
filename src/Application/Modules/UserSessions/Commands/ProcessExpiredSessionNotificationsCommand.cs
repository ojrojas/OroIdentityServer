// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

/// <summary>
/// Closes sessions that ran out of time and queues one "your session expired" email for each.
/// Driven by a scheduled sweep rather than by the request that noticed the expiry, so the
/// notification arrives even if the user's browser was already gone.
/// </summary>
public record ProcessExpiredSessionNotificationsCommand(int MaxBatchSize = 200) : ICommand<ExpiredSessionNotificationSummary>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public sealed record ExpiredSessionNotificationSummary(int Processed, int Notified, int Skipped);