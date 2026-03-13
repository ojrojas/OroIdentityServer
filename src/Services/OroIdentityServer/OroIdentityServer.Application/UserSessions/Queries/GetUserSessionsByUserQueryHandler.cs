// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetUserSessionsByUserQueryHandler(
    ILogger<GetUserSessionsByUserQueryHandler> logger,
    IUserSessionRepository userSessionRepository
) : IQueryHandler<GetUserSessionsByUserQuery, IEnumerable<UserSession>>
{
    public async Task<IEnumerable<UserSession>> HandleAsync(GetUserSessionsByUserQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetUserSessionsByUserQuery for UserId: {UserId}", query.UserId);
        try
        {
            var sessions = await userSessionRepository.GetSessionsByUserIdAsync(query.UserId, cancellationToken);
            logger.LogInformation("Retrieved {Count} sessions for user {UserId}", sessions?.Count() ?? 0, query.UserId);
            return sessions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving sessions for user {UserId}", query.UserId);
            throw;
        }
    }
}
