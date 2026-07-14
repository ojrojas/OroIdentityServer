// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Sessions.Queries;

public class GetUserSessionsQueryHandler(
    ILogger<GetUserSessionsQueryHandler> logger,
    ISessionRepository sessionRepository)
: IQueryHandler<GetUserSessionsQuery, GetUserSessionsQueryResponse>
{
    public async Task<GetUserSessionsQueryResponse> HandleAsync(GetUserSessionsQuery query, CancellationToken cancellationToken)
    {
        var response = new GetUserSessionsQueryResponse();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetUserSessionsQuery for UserId: {UserId}", query.UserId);

            var sessions = await sessionRepository.GetSessionsByUserIdAsync(new(query.UserId), cancellationToken);

        try
        {
            response.Data = sessions.Select(s => new SessionDto
            {
                UserId = s.UserId.Value,
                SessionId = s.Id.Value,
                TenantId = s.TenantId.Value,
                StartedAtUtc = s.StartedAtUtc,
                EndedAtUtc = s.EndedAtUtc,
                AuthorizationId = s.AuthorizationId,
                Country = s.Country,
                IpAddress = s.IpAddress,
                
            });
            logger.LogInformation("Successfully handled GetUserSessionsQuery for UserId: {UserId}", query.UserId);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetUserSessionsQuery for UserId: {UserId}", query.UserId);
            throw;
        }
    }
}
