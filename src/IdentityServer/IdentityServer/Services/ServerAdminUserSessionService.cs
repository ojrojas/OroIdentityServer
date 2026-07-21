using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.UserSessions;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using OroIdentityServer.Application.Modules.UserSessions.Queries;
using OroIdentityServer.Core.Modules.UserSessions.Aggregates;

namespace IdentityServer.Services;

public class ServerAdminUserSessionService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminUserSessionService
{
    public async Task<IEnumerable<UserSessionModel>?> GetActiveSessionsAsync(CancellationToken ct = default)
    {
        var sessions = await queryDispatcher.SendAsync(new GetAllActiveUserSessionsQuery(), ct);
        return sessions.Select(MapUserSession).ToList();
    }

    public async Task<IEnumerable<UserSessionModel>?> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await queryDispatcher.SendAsync(new GetUserSessionsByUserQuery(userId), ct);
        return sessions.Select(MapUserSession).ToList();
    }

    public async Task<HttpResponseMessage> CreateUserSessionAsync(CreateUserSessionRequest request, CancellationToken ct = default)
    {
        var command = new CreateUserSessionCommand(
            request.UserId, request.Device, request.SessionToken, request.ExpiresAt, request.IpAddress, request.UserAgent, request.Location);
        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> DeactivateUserSessionAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeactivateUserSessionCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> TerminateAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new TerminateAllUserSessionsCommand(userId), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default)
        => queryDispatcher.SendAsync(new GetActiveUserSessionsCountQuery(), ct);

    private static UserSessionModel MapUserSession(UserSession session) => new(
        session.Id.Value, session.UserId?.Value, session.Device, session.SessionToken, session.CreatedAt, session.ExpiresAt,
        session.LastActivityAt, session.IpAddress, session.UserAgent, session.Location);
}
