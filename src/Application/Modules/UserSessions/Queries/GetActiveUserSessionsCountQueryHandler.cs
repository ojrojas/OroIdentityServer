// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.UserSessions.Queries;

public class GetActiveUserSessionsCountQueryHandler(
    ILogger<GetActiveUserSessionsCountQueryHandler> logger,
    IUserSessionRepository userSessionRepository
) : IQueryHandler<GetActiveUserSessionsCountQuery, int>
{
    public async Task<int> HandleAsync(GetActiveUserSessionsCountQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetActiveUserSessionsCountQuery");
        var sessions = await userSessionRepository.GetActiveSessionsAsync(cancellationToken);
        return sessions.Select(s => s.UserId).Distinct().Count();
    }
}
