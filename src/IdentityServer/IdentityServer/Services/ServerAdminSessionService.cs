using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Sessions;
using OroIdentityServer.Application.Modules.Sessions.DTOs;
using OroIdentityServer.Application.Modules.Sessions.Queries;

namespace IdentityServer.Services;

public class ServerAdminSessionService(IQueryDispatcher queryDispatcher) : IAdminSessionService
{
    public async Task<ApiResponse<IEnumerable<SessionModel>>?> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetUserSessionsQuery(userId), ct);
        return new ApiResponse<IEnumerable<SessionModel>>
        {
            Data = result.Data?.Select(MapSession).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    private static SessionModel MapSession(SessionDto session) => new(
        session.SessionId, session.UserId, session.TenantId, session.AuthorizationId,
        session.IpAddress, session.Country, session.StartedAtUtc, session.EndedAtUtc);
}
