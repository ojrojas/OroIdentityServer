// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserSessions.Repositories;

public interface IUserSessionRepository
{
    Task AddUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task UpdateUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task DeleteUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task<UserSession?> GetUserSessionByIdAsync(UserSessionId id, CancellationToken cancellationToken);
    Task<IEnumerable<UserSession>> GetSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);
    Task<UserSession?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken);
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sessions that ran past <paramref name="asOf"/> while still open and that have not been
    /// notified yet, with just enough user data to write the notification. Probes the index on
    /// (EndReason, ExpiresAt) and returns at most <paramref name="maxCount"/> rows so one sweep
    /// cannot pull the whole table into memory.
    /// </summary>
    Task<IReadOnlyList<ExpiredSessionInfo>> GetExpiredPendingNotificationAsync(
        DateTime asOf,
        int maxCount,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts distinct users that currently have at least one active (non-expired) session.
    /// </summary>
    Task<int> CountActiveUsersAsync(CancellationToken cancellationToken);
}

/// <summary>Projection used by the expiry notification sweep.</summary>
public sealed record ExpiredSessionInfo(
    Guid SessionId,
    Guid UserId,
    string? UserName,
    string? Email,
    string? Device,
    string? Location,
    string? Culture,
    DateTime ExpiresAt);
