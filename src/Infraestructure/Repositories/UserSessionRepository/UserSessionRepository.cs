// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class UserSessionRepository(
    ILogger<UserSessionRepository> logger,
    IRepository<UserSession> repository,
    OroIdentityAppContext appContext) : IUserSessionRepository
{
    public async Task AddUserSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddUserSessionAsync");
        await repository.AddAsync(session, cancellationToken);
        logger.LogInformation("Exiting AddUserSessionAsync");
    }

    public async Task UpdateUserSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateUserSessionAsync");
        await repository.UpdateAsync(session, cancellationToken);
        logger.LogInformation("Exiting UpdateUserSessionAsync");
    }

    public async Task DeleteUserSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteUserSessionAsync");
        await repository.DeleteAsync(session, cancellationToken);
        logger.LogInformation("Exiting DeleteUserSessionAsync");
    }

    public async Task<UserSession?> GetUserSessionByIdAsync(UserSessionId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetUserSessionByIdAsync");
        var result = await repository.GetByIdAsync(id, cancellationToken);
        logger.LogInformation("Exiting GetUserSessionByIdAsync");
        return result;
    }

    public async Task<IEnumerable<UserSession>> GetSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetSessionsByUserIdAsync");
        var sessions = await repository.FindAsync(s => s.UserId == userId, cancellationToken);
        logger.LogInformation("Exiting GetSessionsByUserIdAsync");
        return sessions;
    }

    public async Task<UserSession?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetByTokenAsync");
        var session = await repository.FindSingleAsync(s => s.SessionToken == sessionToken, cancellationToken);
        logger.LogInformation("Exiting GetByTokenAsync");
        return session;
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetActiveSessionsAsync");
        var sessions = await repository.FindAsync(s => s.ExpiresAt > DateTime.UtcNow, cancellationToken);
        logger.LogInformation("Exiting GetActiveSessionsAsync");
        return sessions;
    }

    public async Task<int> CountActiveUsersAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering CountActiveUsersAsync");
        var sessions = await repository.FindAsync(s => s.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var count = sessions.Select(s => s.UserId).Distinct().Count();
        logger.LogInformation("Exiting CountActiveUsersAsync with {Count} active users", count);
        return count;
    }

    public async Task<IReadOnlyList<ExpiredSessionInfo>> GetExpiredPendingNotificationAsync(
        DateTime asOf,
        int maxCount,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetExpiredPendingNotificationAsync");

        // Two steps instead of a join: joining on a converted strongly-typed id ("UserId.Value") is
        // boxed by the expression tree and EF cannot translate it. Both halves only project
        // primitives, so each one translates on its own.
        var sessions = await appContext.UserSessions
            .AsNoTracking()
            .Where(s => s.EndReason == SessionEndReason.None
                        && s.ExpiryNotifiedAtUtc == null
                        && s.ExpiresAt <= asOf)
            .OrderBy(s => s.ExpiresAt)
            .Take(maxCount)
            .Select(s => new
            {
                SessionId = s.Id.Value,
                UserId = s.UserId!.Value,
                s.Device,
                s.Location,
                s.Culture,
                s.ExpiresAt
            })
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            logger.LogInformation("Exiting GetExpiredPendingNotificationAsync: nothing pending");
            return [];
        }

        var userIds = sessions.Select(s => s.UserId).Distinct().ToList();
        var users = await appContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id.Value))
            .Select(u => new { Id = u.Id.Value, u.Email, u.UserName })
            .ToListAsync(cancellationToken);

        var byId = users.ToDictionary(u => u.Id);

        var rows = sessions
            .Select(s => new ExpiredSessionInfo(
                s.SessionId,
                s.UserId,
                byId.TryGetValue(s.UserId, out var user) ? user.UserName : null,
                byId.TryGetValue(s.UserId, out var addressee) ? addressee.Email : null,
                s.Device,
                s.Location,
                s.Culture,
                s.ExpiresAt))
            .ToList();

        logger.LogInformation("Exiting GetExpiredPendingNotificationAsync with {Count} sessions", rows.Count);
        return rows;
    }
}
