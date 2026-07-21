// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
using OroIdentityServer.Core.Modules.UserSessions.Repositories;

namespace OroIdentityServer.Application.Modules.UserSessions.Queries;

public class GetAllActiveUserSessionsQueryHandler(
    ILogger<GetAllActiveUserSessionsQueryHandler> logger,
    IUserSessionRepository userSessionRepository
) : IQueryHandler<GetAllActiveUserSessionsQuery, IEnumerable<UserSession>>
{
    public async Task<IEnumerable<UserSession>> HandleAsync(GetAllActiveUserSessionsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetAllActiveUserSessionsQuery");
        return await userSessionRepository.GetActiveSessionsAsync(cancellationToken);
    }
}
