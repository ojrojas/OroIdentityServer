// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Core.Models;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session, CancellationToken cancellationToken);
    Task EndSessionAsync(SessionId sessionId, DateTime endedAtUtc, CancellationToken cancellationToken);
    Task<Session?> GetSessionByIdAsync(SessionId sessionId, CancellationToken cancellationToken);
    Task<IEnumerable<Session>> GetSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}
