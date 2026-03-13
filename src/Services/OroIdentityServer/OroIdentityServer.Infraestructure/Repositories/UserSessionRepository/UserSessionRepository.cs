// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class UserSessionRepository(
    ILogger<UserSessionRepository> logger,
    IRepository<UserSession> repository) : IUserSessionRepository
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
        var sessions = await repository.CurrentContext.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        logger.LogInformation("Exiting GetSessionsByUserIdAsync");
        return sessions;
    }

    public async Task<UserSession?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetByTokenAsync");
        var session = await repository.CurrentContext.FirstOrDefaultAsync(s => s.SessionToken == sessionToken, cancellationToken);
        logger.LogInformation("Exiting GetByTokenAsync");
        return session;
    }
}
