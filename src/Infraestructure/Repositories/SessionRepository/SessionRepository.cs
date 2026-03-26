// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class SessionRepository(
    ILogger<SessionRepository> logger,
    IRepository<Session> repository,
    OroIdentityAppContext context) : ISessionRepository
{
    public async Task AddSessionAsync(Session session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddSessionAsync");
        await repository.AddAsync(session, cancellationToken);
        logger.LogInformation("Exiting AddSessionAsync");
    }

    public async Task EndSessionAsync(SessionId sessionId, DateTime endedAtUtc, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering EndSessionAsync with id: {Id}", sessionId);
        var session = await repository.GetByIdAsync(sessionId, cancellationToken);
        if (session != null)
        {
            session.End(endedAtUtc);
            await repository.UpdateAsync(session, cancellationToken);
        }
        logger.LogInformation("Exiting EndSessionAsync");
    }

    public async Task<Session?> GetSessionByIdAsync(SessionId sessionId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetSessionByIdAsync with id: {Id}", sessionId);
        var result = await repository.GetByIdAsync(sessionId, cancellationToken);
        logger.LogInformation("Exiting GetSessionByIdAsync");
        return result;
    }

    public async Task<IEnumerable<Session>> GetSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetSessionsByUserIdAsync for userId: {UserId}", userId);
        var result = await repository.FindAsync(s => s.UserId == userId, cancellationToken);
        logger.LogInformation("Exiting GetSessionsByUserIdAsync");
        return result;
    }
}
